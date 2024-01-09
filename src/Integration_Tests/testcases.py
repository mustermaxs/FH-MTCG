from api_utils import *
from utils import *
from models import *
from helpers import *
import random

stats = {
    "passed": 0,
    "failed": 0
}

@with_caller_name
def test_status(res, expected_status, doAssert=False, cn=None):
    if cn != None:
        cn = cn.replace("_", " ").replace("test", "")
    if (res.status_code != expected_status):
        print_colored(f" \nFAILED {cn:<35} : {res.status_code} : {res.reason}\n", Colors.RED)
        stats["failed"] += 1
        if doAssert:
            assert res.status_code == expected_status
        return False
    else:
        print_colored(f" PASSED {cn:<35} : {res.status_code} - {res.reason}", Colors.GREEN)
        stats["passed"] += 1
        if doAssert:
            assert res.status_code == expected_status
        return True


@with_caller_name
def test_rename_user(cn=None):
    user = login_as(users["max"])
    user_res = get_user_by_id(user.ID)
    user.Name = "klax"
    user.Bio = "geaendert"
    user.Image = ":("
    res = req.put(url("users", "PUT"), json=user.to_dict(), headers=Headers(user.token))
    test_status(res, 200, True)


@with_caller_name
def custom_assert(isTrue: bool, doAssert=False, msg="", cn=None):
    if not isTrue:
        print_colored(f" FAILED {cn:<35} : {msg}", Colors.RED)
        stats["failed"] += 1
        if doAssert:
            assert isTrue
    else:
        print_colored(f" PASSED {cn:<35} : {msg}", Colors.GREEN)
        stats["passed"] += 1
        if doAssert:
            assert isTrue





@with_caller_name
def add_card_globally(card: Card):
    reset()
    admin = login_as(users["admin"])
    res = req.post(url("cards", "POST"), json=card.to_dict(), headers=Headers(admin.token))
    cards = test_getall_cards()
    
    custom_assert(len(cards) > 0 and res.status_code == 201, True)



@with_caller_name
def test_login(user : User, cn=None):
    creds = {"Name": user.Name, "Password": user.Password}
    res = req.post(url("session", "POST"), json=creds)

    if test_status(res, 200, True):        
        return res.json()["authToken"]







@with_caller_name
def test_retrieve_packages_has_packages(cn=None):
    res = req.get(url("packages", "GET"))
    
    custom_assert(res.status_code == 200 and len(res.json()) > 0, True)




@with_caller_name  
def test_retrieve_packages_no_packages(cn=None):
    reset()
    res = req.get(url("packages", "GET"))
    # assert res.status_code == 204
    custom_assert(res.status_code == 204, True)




@with_caller_name
def test_delete_package(id : str, cn=None):
    reset()

    create_package(cards)
    test_login(users["admin"])
    u = url("packages", "DELETE")
    u = u.replace(":id", id)
    res = req.delete(u, headers=Headers(users["admin"].token))
    packages = get_all_packages()
    custom_assert(len(packages) == 0, True)


@with_caller_name
def test_create_package(cn=None):
    reset()

    res = create_package(cards)
    packages = get_all_packages()

    custom_assert(res.status_code == 200 and len(packages) == 1)




@with_caller_name
def test_aquire_package(cn=None):
    reset()
    # delete preexisting packages
    all_packages = get_all_packages()
    if all_packages is not None:
        for package in all_packages:
            delete_package(package["Id"])
    
    # create package
    create_package(cards)

    # buy package
    buyer = login_as(users["test"])
    res = req.post(url("transaction_packages", "POST"), headers=Headers(buyer.token))

    #check if package was deleted
    packages = get_all_packages()
    packages_count = len(packages)
    package_was_deleted = packages_count == 0

    stackcards = get_user_stack(buyer)
    stackcards_count = len(stackcards)
    user_got_cards = stackcards_count > 0

    custom_assert(user_got_cards and package_was_deleted)




@with_caller_name
def test_register_user(cn=None):
    users_before_registration = get_all_users()
    count_before = len(users_before_registration)
    
    test_user = users["registration_test_user"]
    res = req.post(url("users", "POST"), json=test_user.to_dict())

    is_success_response = res.status_code == 201
    users_after_registration = get_all_users()
    count_after = len(users_after_registration)

    custom_assert(count_before + 1 == count_after and is_success_response, True)




@with_caller_name
def test_register_alreadyexisting_user(cn=None):
    user_count_before = len(get_all_users())
    user = users["test"]
    
    res = req.post(url("users", "POST"), json=user.to_dict())
    
    user_count_after = len(get_all_users())

    custom_assert(user_count_before == user_count_after and res.status_code == 500, True)
    




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
    users = get_all_users()
    
    custom_assert(len(users) > 0, True)




@with_caller_name
def test_getall_cards(cn=None):
    res = req.get(url("all_cards", "GET"))
    if test_status(res, 200):
        return res.json()




@with_caller_name
def test_get_user_stack(user: User, cn=None):
    test_login(user)
    res = req.get(url("stack", "GET"), headers=Headers(user.token))
    if test_status(res, 200, True):
        return res.json()
    else:
        raise Exception("Could not get stack")




@with_caller_name
def test_aquire_package_and_create_deck(user: User, cards: list, cn=None):
    reset()
    admintoken = login_as(users["admin"])
    packageid = create_package(cards)
    user = login_as(user)
    aquire_package( user, packageid)
    stackcards = get_user_stack(user)
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

    dealer = users["max"]
    buyer = users["test"]

    create_package(cards)
    packages = get_all_packages()
    aquire_package(dealer, packages[0]["Id"])
    stackcards = get_user_stack(dealer)
    push_cards_to_deck(dealer, stackcards)

    create_package(cards)
    packages = get_all_packages()
    aquire_package(buyer, packages[0]["Id"])
    buyerstackcards = get_user_stack(buyer)
    push_cards_to_deck(buyer, buyerstackcards)

    add_cardtrade_deal(dealer, stackcards[0])

    pendingDeals = get_cardtrades()
    
    buyerDeck = get_user_deck(buyer)
    offeredCardForDeal = buyerDeck[0]["Id"]

    URL = url("accept_card_trade", "POST").replace(":id", pendingDeals[0]["Id"])
    buyer = login_as(buyer)
    res = req.post(URL, json=offeredCardForDeal, headers=Headers(buyer.token))
    test_status(res, 200, True)




@with_caller_name
def test_post_cardtrade(cn=None):
    pass




# @with_caller_name
# def delete_all_cards(cn=None):
#     admin = login_as(users["admin"])
#     res = req.delete(url("cards_all", "DELETE"), headers=Headers(admin.token))
#     test_status(res, 200, True)




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
    custom_assert(success==True, "Card was not added to stack")