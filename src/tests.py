import requests
import unittest
import json
import pickle

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

def expect_status(res, expected_status, doAssert=False):
    if (res.status_code is not expected_status):
        print_colored(f" FAILED {res.status_code} : {res.reason}", Colors.RED)
        if doAssert:
            assert res.status_code == expected_status
        return False
    else:
        print_colored(f" PASSED {res.requested_method} {res.requested_url} : {res.reason}", Colors.GREEN)
        if doAssert:
            assert res.status_code == expected_status
        return True


def test_login(user : User):
    creds = {"Name": user.Name, "Password": user.Password}
    res = req.post(url("session", "POST"), json=creds)

    if expect_status(res, 200, True):        
        return res.json()["authToken"]





def test_retrieve_packages_has_packages():
    res = req.get(url("packages", "GET"))
    assert res.status_code == 200

    if expect_status(res, 200):
        return res.json()
    else:
        return None
    
def test_retrieve_packages_no_packages():
    res = req.get(url("packages", "GET"))
    assert res.status_code == 204

    if res.status_code == 204:
        print("Packages retrieved successfully. No packages exist.")

def test_delete_package(id):
    test_login(users["admin"])
    u = url("packages", "DELETE")
    u = u.replace(":id", id)
    res = req.delete(u, headers=Headers(users["admin"].token))
    expect_status(res, 200)


# Returns package id
def test_create_package(cards : list, user : User):
    cards_list = [card.to_dict() for card in cards.values()]
    token = test_login(user.token)
    res = req.post(url("packages", "POST"), json=cards_list, headers=Headers(token))
    
    if expect_status(res, 200, True):
        packages = test_retrieve_packages_has_packages()
        packageId = packages[0]["Id"]
        return packageId


def test_aquire_package(id : str):
    # delete preexisting packages
    packages = test_retrieve_packages_has_packages()
    for package in packages:
        test_delete_package(package["Id"])
    
    # create package
    test_create_package(cards, users["admin"])

    # buy package
    test_login(users["max"])
    res = req.post(url("transactions/packages", "POST"), headers=Headers(users["max"].token))

    expect_status(res, 200)

def test_register_user(user: User):
    res = req.post(url("users", "POST"), json=user.to_dict())
    if expect_status(res, 201, True):
        response = res.json()
        user.token = response["authToken"]
        return user
    else:
        return None
    
def test_register_alreadyexisting_user(user: User):
    res = req.post(url("users", "POST"), json=user.to_dict())
    if expect_status(res, 500, True):
        response = res.json()
        user.token = response["authToken"]
        return user
    else:
        return None

test_user = User("test", "test", "", ":)", 100, "", "")

test_user = test_register_user(test_user)
test_register_alreadyexisting_user(test_user)
test_retrieve_packages_no_packages()
test_login(users["max"])
test_create_package(cards, users["max"])
packages = test_retrieve_packages_has_packages()

if packages is not None:
    test_delete_package(packages[0]["Id"])
# test_create_package(cards, users["admin"])
# test_aquire_package()