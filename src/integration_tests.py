import requests
import unittest
import json
import pickle
import inspect

api = {
    "GET": {
        "stack": "/stack",
        "deck": "/deck",
        "cards": "/cards/{cardId:alphanum}",
        "all_cards": "/cards/all",
        "packages": "/packages",
        "package": "/packages/{packageid:alphanum}",
        "tradings": "/tradings",
        "users": "/users/",
        "user_by_id": "/users/{username:alphanum}",
        "user_by_name": "/users/{username:alpha}",
        "score": "/score"
    },
    "PUT": {
        "deck": "/deck",
        "users": "/users/{username:alphanum}"
    },
    "POST": {
        "cards": "/cards",
        "packages": "/packages",
        "transaction_packages": "/transactions/packages",
        "tradings": "/tradings",
        "session": "/session/",
        "users": "/users/",
        "users_settings_language": "/users/settings/language"
    },
    "DELETE": {
        "tradings": "/tradings/{tradingdealid:alphanum}",
        "packages": "/packages/:id"
    }
}


# Base URL for the API
base_url = "http://localhost:12000"  # Replace "your_port" with the actual port


def url(endpoint, method):
    ep = api[method][endpoint]
    return base_url + ep


# Bearer token for authentication
bearer_token = "7944947a9f442df12c947d07c71221d3ccf929fdc02837d74baec3e1b47dc1a1"

# class Headers:
#     def __init__(self, token):
#         self.Accept = "application/json"
#         self.AuthToken = f"Bearer {token}"
    
#     def setAuthToken(self, token):
#         self.AuthToken = f"Bearer {token}"
#         return self
def Headers(token):
    return  {
    "Accept": "application/json",
    "Authorization": f"Bearer {token}"}
# Headers
headers = {
    "Accept": "application/json",
    "Authorization": f"Bearer {bearer_token}"
}

test_users = {
    "admin": {
        "Name": "admin",
        "Password": "admin",
        "Bio": "admin bio",
        "Image": ":)",
        "Coins": 100,
        "ID": ""
    }
}

class Card:
    def __init__(self, description, damage, name, element, type):
        self.Description = description
        self.Damage = damage
        self.Name = name
        self.Element = element
        self.Type = type

    def to_dict(self):
        return {
            "Description": self.Description,
            "Damage": self.Damage,
            "Name": self.Name,
            "Element": self.Element,
            "Type": self.Type
        }
    
class Trade:
    def __init__(self, card_to_trade, type, minimum_damage):
        self.CardToTrade = card_to_trade
        self.Type = type
        self.MinimumDamage = minimum_damage

    def to_dict(self):
        return {
            "CardToTrade": self.CardToTrade,
            "Type": self.Type,
            "MinimumDamage": self.MinimumDamage
        }

trade = Trade("firekraken", "fire", 10.0)

firekraken = Card("", 20.0, "FireKraken", "fire", "monster")
firetroll = Card("", 10.0, "FireTroll", "fire", "monster")
firespell = Card("", 15.0, "FireSpell", "fire", "monster")
waterspell = Card("", 10.0, "WaterSpell", "water", "spell")
regularspell = Card("", 20.0, "RegularSpell", "normal", "spell")

cards = {
    "firekraken": firekraken,
    "firetroll": firetroll,
    "firespell": firespell,
    "waterspell": waterspell,
    "regularspell": regularspell
}





class User:
    def __init__(self, name, password, bio, image, coins, id, token):
        self.Name = name
        self.Password = password
        self.Bio = bio
        self.Image = image
        self.Coins = coins
        self.ID = id
        self.token = token

    def to_dict(self):
        return {
            "Name": self.Name,
            "Password": self.Password,
            "Bio": self.Bio,
            "Image": self.Image,
            "Coins": self.Coins
            }

admin = User("admin", "admin", "admin bio", ":)", 100, "14076953-be7d-4bfb-9180-a797d9dad345", "33b039518718ba03b03e08e473032c9c4d72a2f9d5b27a43938c1bc49d4598a3")
normal_user = User("max", "max", "", ":)", 100, "d324b594-06d6-42e2-b921-f371180edb8b", "7944947a9f442df12c947d07c71221d3ccf929fdc02837d74baec3e1b47dc1a1")

users = {
    "admin": admin,
    "max": normal_user
}

class CustomRequests:
    def __init__(self, base_url):
        self.base_url = base_url

    def get(self, endpoint, **kwargs):
        return self._make_request("GET", endpoint, **kwargs)

    def post(self, endpoint, **kwargs):
        return self._make_request("POST", endpoint, **kwargs)

    def delete(self, endpoint, **kwargs):
        return self._make_request("DELETE", endpoint, **kwargs)

    def _make_request(self, method, endpoint, **kwargs):
        url = endpoint
        response = requests.request(method, url, **kwargs)
        response.requested_url = url
        response.requested_method = method

        return response
    
req = CustomRequests(base_url)

class Colors:
    RESET = '\033[0m'
    RED = '\033[91m'
    GREEN = '\033[92m'
    YELLOW = '\033[93m'
    BLUE = '\033[94m'
    PURPLE = '\033[95m'
    CYAN = '\033[96m'

def print_colored(text, color):
    print(f"{color}{text}{Colors.RESET}")

def with_caller_name(func):
    def wrapper(*args, **kwargs):
        cn = inspect.currentframe().f_back.f_code.co_name
        kwargs['cn'] = cn
        return func(*args, **kwargs)
    return wrapper


@with_caller_name
def test_status(res, expected_status, doAssert=False, cn=None):
    if (res.status_code != expected_status):
        print_colored(f" FAILED {cn:<40} : {res.status_code} : {res.reason}", Colors.RED)
        if doAssert:
            assert res.status_code == expected_status
        return False
    else:
        print_colored(f" PASSED {cn:<40} : {res.status_code} - {res.reason}", Colors.GREEN)
        if doAssert:
            assert res.status_code == expected_status
        return True

def login_as(user: User):
    creds = {"Name": user.Name, "Password": user.Password}
    res = req.post(url("session", "POST"), json=creds)

    if res.status_code == 200:        
        user.token = res.json()["authToken"]
        return user


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


# Returns package id
@with_caller_name
def test_create_package(cards : list, user : User, cn=None):
    cards_list = [card.to_dict() for card in cards.values()]
    admin = login_as(users["admin"])
    res = req.post(url("packages", "POST"), json=cards_list, headers=Headers(admin.token))
    
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

def test_create_deck(user: User, cards: list, cn=None):
    # setup
    test_login(user)
    admintoken = login_as(users["admin"])
    packageid = test_create_package(cards, users["admin"])
    test_aquire_package( user)
    stackcards = test_get_user_stack(user)
    cards_list = [card["Id"] for card in stackcards]
    res = req.put(url("deck", "PUT"), json=cards_list, headers=Headers(user.token))
    test_status(res, 200)



@with_caller_name
def test_add_trading_deal(user: User, deal=trade , cn=None):
    # setup
    test_login(user)
    packageid = test_create_package(cards, users["admin"])
    test_aquire_package( user)

    res = req.post(url("tradings", "POST"), json=deal, headers=Headers(user.token))
    

test_user = User("test", "test", "", ":)", 100, "", "")

# test_user = test_register_user(test_user)
# SETUP
delete_all_packages()

test_register_alreadyexisting_user(test_user)
test_retrieve_packages_no_packages()
test_login(users["max"])
test_create_package(cards, users["admin"])
packages = test_retrieve_packages_has_packages()

if packages is not None:
    test_delete_package(packages[0]["Id"])
test_get_all_users()
test_user_no_cards_in_stack_true(users["max"])
test_aquire_package( users["max"])
test_get_user_stack(users["max"])
test_create_deck(users["max"], cards)