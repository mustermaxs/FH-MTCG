from api_utils import *
from utils import *
from models import *
import random


settings = {
    "changedLang": False
}

def reset():
    # delete_all_cards()
    # delete_all_packages()
    login_as(users["admin"])
    res = req.post(url("reset", "POST"), headers=Headers(admin.token))
    assert res.status_code == 200

def delete_all_cards():
    admin = login_as(users["admin"])
    res = req.delete(url("cards_all", "DELETE"), headers=Headers(admin.token))

def change_language(lang: str, user: User):
    # login_as(user)
    URL = url("language", "PUT").replace(":lang", lang)
    res = req.put(URL, headers=Headers(user.token))


def login_as(user: User):
    if user.Name == "max" and not settings["changedLang"]:
        change_language("german", user)
        settings["changedLang"] = True
    creds = {"Name": user.Name, "Password": user.Password}
    res = req.post(url("session", "POST"), json=creds)
    return user
    if res.status_code == 200:        
        user.token = res.json()["authToken"]
        return user
    else:
        assert res.status_code == 200

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
    # print(user.token)
    login_as(user)
    res = req.get(url("stack", "GET"), headers=Headers(user.token))
    if res.status_code == 200:
        return res.json()
    else:
        return []

def save_card(card: Card):
    admin = login_as(users["admin"])
    # print(card)
    res = req.post(url("cards", "POST"), json=card.to_dict(), headers=Headers(admin.token))
    assert res.status_code == 200
    
    return res




def aquire_package(user: User, packageId: str):
    buyer = login_as(user)
    res = req.post(url("transaction_packages", "POST"), headers=Headers(buyer.token))
    assert res.status_code == 200

def push_cards_to_deck(user: User, cards):
    user = login_as(user)
    cards_list = [card["Id"] for card in cards]
    res = req.put(url("deck", "PUT"), json=cards_list, headers=Headers(user.token))
    assert res.status_code == 200

def add_cardtrade_deal(user: User, card: Card):
    # print(card)
    deal = Trade(card["Id"], card["Type"], card["Damage"])
    res = req.post(url("tradings", "POST"), json=deal.to_dict(), headers=Headers(user.token))
    assert res.status_code == 201

def get_cardtrades():
    res = req.get(url("tradings", "GET"))
    if res.status_code == 200:
        return res.json()
    elif res.status_code == 204:
        return []
    else:
        assert res.status_code == 200

def card_meets_deal_requirements(card, dealcard):
    if card["Type"] == dealcard["Type"] and card["Damage"] == dealcard["MinimumDamage"]:
        return True
    else:
        return False

# json cards
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


def get_all_cards():
    admin = login_as(users["admin"])
    res = req.get(url("all_cards", "GET"), headers=Headers(admin.token))
    if res.status_code == 200:
        return res.json()
    else:
        assert res.status_code == 200

def put_cards_in_deck(user: User, all_cards):
    admin = login_as(users["admin"])
    # print(all_cards)
    for card in all_cards:
        save_card(card)
        
    all_cards = get_all_cards()
    add_cards_to_stack(all_cards, user)
    push_cards_to_deck(user, all_cards)

def delete_user(id: str):
    admin = login_as(users["admin"])
    URL = url("user", "DELETE").replace(":id", id)
    res = req.delete(URL, headers=Headers(admin.token))
    assert res.status_code == 200
    return res