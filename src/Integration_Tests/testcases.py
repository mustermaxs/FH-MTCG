from api_utils import *
from utils import *
from models import *
import random

def reset():
    delete_all_cards()
    delete_all_packages()

@with_caller_name
def test_status(res, expected_status, doAssert=False, cn=None):
    if (res.status_code != expected_status):
        print_colored(f" \nFAILED {cn:<35} : {res.status_code} : {res.reason}\n", Colors.RED)
        if doAssert:
            assert res.status_code == expected_status
        return False
    else:
        print_colored(f" PASSED {cn:<35} : {res.status_code} - {res.reason}", Colors.GREEN)
        if doAssert:
            assert res.status_code == expected_status
        return True

@with_caller_name
def custom_assert(expect, actual, doAssert=False, msg="", cn=None):
    if type(expect) == "Response":
        return test_status(expect, actual, doAssert, cn)
    if expect != actual:
        print_colored(f" FAILED {cn:<35} : {msg}", Colors.RED)
        if doAssert:
            assert expect == actual
    else:
        print_colored(f" PASSED {cn:<35} : {msg}", Colors.GREEN)
        if doAssert:
            assert expect == actual


def login_as(user: User):
    creds = {"Name": user.Name, "Password": user.Password}
    res = req.post(url("session", "POST"), json=creds)

    if res.status_code == 200:        
        user.token = res.json()["authToken"]
        return user

@with_caller_name
def add_card_globally(card: Card):
    admin = login_as(users["admin"])
    res = req.post(url("cards", "POST"), json=card.to_dict(), headers=Headers(admin.token))
    test_status(res, 201, True)
@with_caller_name
def test_login(user : User, cn=None):
    creds = {"Name": user.Name, "Password": user.Password}
    res = req.post(url("session", "POST"), json=creds)

    if test_status(res, 200, True):        
        return res.json()["authToken"]




@with_caller_name
def test_retrieve_packages_has_packages(cn=None):
    res = req.get(url("packages", "GET"))
    assert res.status_code == 200

    if test_status(res, 200):
        return res.json()
    else:
        return None

@with_caller_name  
def test_retrieve_packages_no_packages(cn=None):
    res = req.get(url("packages", "GET"))
    # assert res.status_code == 204
    test_status(res, 204, True)

@with_caller_name
def test_delete_package(id : str, cn=None):
    test_login(users["admin"])
    u = url("packages", "DELETE")
    u = u.replace(":id", id)
    res = req.delete(u, headers=Headers(users["admin"].token))
    test_status(res, 200)

# returns response
def create_package(cards):
    cards_list = [card.to_dict() for card in cards.values()]
    admin = login_as(users["admin"])
    res = req.post(url("packages", "POST"), json=cards_list, headers=Headers(admin.token))
    return res


# Returns package id
@with_caller_name
def test_create_package(cards : list, user : User, cn=None):
    res = create_package(cards)

    if test_status(res, 200, True):
        packages = test_retrieve_packages_has_packages()
        packageId = packages[0]["Id"]
        return packageId

@with_caller_name
def test_aquire_package( user: User, cn=None):
    # delete preexisting packages
    packages = get_all_packages()
    if packages is not None:
        for package in packages:
            test_delete_package(package["Id"])
    
    # create package
    test_create_package(cards, users["admin"])

    # buy package
    buyer = login_as(user)
    res = req.post(url("transaction_packages", "POST"), headers=Headers(buyer.token))
    test_status(res, 200)
    test_retrieve_packages_no_packages()

@with_caller_name
def test_register_user(user: User , cn=None):
    res = req.post(url("users", "POST"), json=user.to_dict())
    if test_status(res, 201, True):
        response = res.json()
        user.token = response["authToken"]
        return user
    else:
        return None

@with_caller_name
def test_register_alreadyexisting_user(user: User, cn=None):
    res = req.post(url("users", "POST"), json=user.to_dict())
    if test_status(res, 500, True):
        return user
    else:
        return None
    

@with_caller_name  
def test_user_no_cards_in_stack_true(user: User, cn=None):
    test_login(user)
    res = req.get(url("deck", "GET"), headers=Headers(user.token))
    test_status(res, 204)

@with_caller_name
def test_user_cards_in_deck_true(user: User, cn=None):
    test_login(user)
    res = req.get(url("deck", "GET"), headers=Headers(user.token))
    test_status(res, 200, True)
    return res.json()



@with_caller_name
def test_get_all_users(cn=None):
    admin = login_as(users["admin"])
    res = req.get(url("users", "GET"), headers=Headers(admin.token))
    if test_status(res, 200):
        return res.json()

@with_caller_name
def test_getall_cards(cn=None):
    res = req.get(url("all_cards", "GET"))
    if test_status(res, 200):
        return res.json()


def get_all_packages():
    res = req.get(url("packages", "GET"))
    if res.status_code == 200:
        return res.json()
    else:
        return None

def delete_all_packages():
    packages = get_all_packages()
    if packages is None:
        return
    for package in packages:
        test_delete_package(package["Id"])


@with_caller_name
def test_get_user_stack(user: User, cn=None):
    test_login(user)
    res = req.get(url("stack", "GET"), headers=Headers(user.token))
    if test_status(res, 200):
        return res.json()

@with_caller_name
def test_aquire_package_and_create_deck(user: User, cards: list, cn=None):
    # setup
    admintoken = login_as(users["admin"])
    packageid = test_create_package(cards, users["admin"])
    user = login_as(user)
    test_aquire_package( user)
    stackcards = test_get_user_stack(user)
    cards_list = [card["Id"] for card in stackcards]
    res = req.put(url("deck", "PUT"), json=cards_list, headers=Headers(user.token))
    test_status(res, 200)

@with_caller_name
def get_user_deck(user: User, cn=None):
    user = login_as(user)
    res = req.get(url("deck", "GET"), headers=Headers(user.token))
    if test_status(res, 200, True):
        return res.json()


@with_caller_name
def test_add_trading_deal(user: User, deal=None , cn=None):
    # setup
    # reset()
    user = login_as(user)
    test_aquire_package_and_create_deck(user, cards)

    if deal is None:
        deckCards = get_user_deck(user)
        card = deckCards[0]
        deal = Trade(card["Id"], card["Type"], card["Damage"])

    res = req.post(url("tradings", "POST"), json=deal.to_dict(), headers=Headers(user.token))
    test_status(res, 201, True)

@with_caller_name
def test_accept_cardtrade_deal(cn=None):
    reset()
    test_aquire_package_and_create_deck(users["test"], cards)
    test_add_trading_deal(users["test"])
    test_aquire_package_and_create_deck(users["max"], cards)
    pendingdeals = get_cardtrades().json()
    
    deck = get_user_deck(users["max"])
    payload = deck[0]["Id"]
    URL = url("accept_card_trade", "POST").replace(":id", pendingdeals[0]["Id"])
    user = login_as(users["max"])
    res = req.post(URL, json=payload, headers=Headers(user.token))
    test_status(res, 200, True)

@with_caller_name
def test_post_cardtrade(cn=None):
    pass

@with_caller_name
def delete_all_cards(cn=None):
    admin = login_as(users["admin"])
    res = req.delete(url("cards_all", "DELETE"), headers=Headers(admin.token))
    test_status(res, 200, True)

def get_cardtrades():
    res = req.get(url("tradings", "GET"))
    if res.status_code == 200:
        return res
    else:
        return None

@with_caller_name
def test_get_cardtrades_exists(cn=None):
    res = get_cardtrades()
    
    if test_status(res, 200, True):
        return res.json()

@with_caller_name
def test_add_card_to_stack(user: User, card, cn=None): # card is json object
    admin = login_as(users["admin"])
    URL = url("add_to_stack", "POST").replace(":id", user.ID)
    cardlist = []
    cardlist.append(card)
    res = req.post(URL, json=cardlist, headers=Headers(admin.token))
    test_status(res, 200, True)

@with_caller_name
def get_card_by_id(id : str, cn=None):
    admin = login_as(users["admin"])
    URL = url("card_by_id", "GET").replace(":id", id)
    res = req.get(URL, headers=Headers(admin.token))
    if test_status(res, 200, True):
        return res.json()



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


@with_caller_name
def test_add_card_to_stack(cn=None):
    reset()
    packageres = create_package(cards)
    if packageres.status_code != 200:
        raise Exception("Could not create package")

    all_cards = test_getall_cards()
    add_cards_to_stack(all_cards, users["test"])
    randomcard = all_cards[random.randint(0, len(all_cards)-1)]  
    stack = test_get_user_stack(users["test"])
    # print(stack)
    # print(all_cards)
    success = True
    if len(stack) < len(all_cards):
        raise Exception("Card was not added to stack")

    for stackcard in stack:
        if any(stackcard["Name"] != stackcard["Name"] for card in cards):
            success = False
            break
    custom_assert(True, success, "Card was not added to stack")