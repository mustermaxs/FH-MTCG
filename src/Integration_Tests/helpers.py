from api_utils import *
from utils import *
from models import *
import random

def reset():
    delete_all_cards()
    delete_all_packages()

def delete_all_cards():
    admin = login_as(users["admin"])
    res = req.delete(url("cards_all", "DELETE"), headers=Headers(admin.token))

def login_as(user: User):
    creds = {"Name": user.Name, "Password": user.Password}
    res = req.post(url("session", "POST"), json=creds)

    if res.status_code == 200:        
        user.token = res.json()["authToken"]
        return user

# returns response
def create_package(cards):
    cards_list = [card.to_dict() for card in cards.values()]
    admin = login_as(users["admin"])
    res = req.post(url("packages", "POST"), json=cards_list, headers=Headers(admin.token))

    if res.status_code == 200:
        return res
    else:
        assert res.status_code == 200

def get_all_packages():
    res = req.get(url("packages", "GET"))
    if res.status_code == 200:
        return res.json()
    else:
        return []

def delete_all_packages():
    packages = get_all_packages()
    if packages is None:
        return
    for package in packages:
        delete_package(package["Id"])

def delete_package(id: str):
    admin = login_as(users["admin"])
    u = url("packages", "DELETE")
    u = u.replace(":id", id)
    res = req.delete(u, headers=Headers(admin.token))

def get_user_stack(user: User):
    res = req.get(url("stack", "GET"), headers=Headers(user.token))
    
    return res.json()


def aquire_package(user: User, packageId: str):
    buyer = login_as(user)
    res = req.post(url("transaction_packages", "POST"), headers=Headers(buyer.token))


def push_cards_to_deck(user: User, cards):
    user = login_as(user)
    cards_list = [card["Id"] for card in cards]
    res = req.put(url("deck", "PUT"), json=cards_list, headers=Headers(user.token))
    assert res.status_code == 200

def add_cardtrade_deal(user: User, card: Card):
    deal = Trade(card["Id"], card["Type"], card["Damage"])
    res = req.post(url("tradings", "POST"), json=deal.to_dict(), headers=Headers(user.token))
    assert res.status_code == 201

def get_cardtrades():
    res = req.get(url("tradings", "GET"))
    if res.status_code == 200:
        return res.json()
    else:
        assert res.status_code == 200

def card_meets_deal_requirements(card, dealcard):
    if card["Type"] == dealcard["Type"] and card["Damage"] == dealcard["MinimumDamage"]:
        return True
    else:
        return False
    
def add_cards_to_stack(cards, user: User):
    admin = login_as(users["admin"])
    URL = url("add_to_stack", "POST").replace(":id", user.ID)
    res = req.post(URL, json=cards, headers=Headers(admin.token))
    # test_status(res, 200, True)
    if res.status_code != 200:
        raise Exception("Could not add cards to stack")

def get_user_by_id(id: str):
    admin = login_as(users["admin"])
    URL = url("user_by_id", "GET").replace(":id", id)
    res = req.get(URL, headers=Headers(admin.token))
    if res.status_code == 200:
        return res.json()
    else:
        return None

def get_all_users():
    admin = login_as(users["admin"])
    res = req.get(url("users", "GET"), headers=Headers(admin.token))
    if res.status_code == 200:
        return res.json()
    else:
        assert res.status_code == 200