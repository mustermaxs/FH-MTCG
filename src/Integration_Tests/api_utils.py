import socket
import requests
import json

base_url = "http://localhost:12000"
ip = "127.0.0.1"
port = 12000

with open("./src/config.json", "r") as file:
    content = json.load(file)
    port = content["server"]["SERVER_PORT"]
    ip = content["server"]["SERVER_IP"]


bearer_token = "7944947a9f442df12c947d07c71221d3ccf929fdc02837d74baec3e1b47dc1a1"

api = {
    "GET": {
        "stack": "/stack",
        "deck": "/deck",
        "card_by_id": "/cards/:id",
        "all_cards": "/cards/all",
        "packages": "/packages",
        "package": "/packages/{packageid:alphanum}",
        "tradings": "/tradings",
        "users": "/users/",
        "user_by_id": "/users/:id",
        "user_by_name": "/users/{username:alpha}",
        "score": "/score"
    },
    "PUT": {
        "deck": "/deck",
        "users": "/users/:id"
    },
    "POST": {
        "cards": "/cards",
        "packages": "/packages",
        "transaction_packages": "/transactions/packages",
        "tradings": "/tradings",
        "accept_card_trade": "/tradings/:id",
        "session": "/session/",
        "users": "/users/",
        "users_settings_language": "/users/settings/language",
        "add_to_stack": "/stack/:id",
        "add_card_glob": "/cards"
    },
    "DELETE": {
        "tradings": "/tradings/:id",
        "packages": "/packages/:id",
        "cards_all": "/cards",
        "user": "/users/:id"
    }
}


def Headers(token):
    return  {
    "Accept": "application/json",
    "Authorization": f"Bearer {token}"}


# Headers

headers = {
    "Accept": "application/json",
    "Authorization": f"Bearer {bearer_token}"
}

###########################################################

def url(endpoint, method):
    ep = api[method][endpoint]
    return base_url + ep

###########################################################



class CustomRequests:
    def __init__(self, base_url):
        self.base_url = base_url

    def get(self, endpoint, **kwargs):
        return self._make_request("GET", endpoint, **kwargs)

    def post(self, endpoint, **kwargs):
        return self._make_request("POST", endpoint, **kwargs)

    def delete(self, endpoint, **kwargs):
        return self._make_request("DELETE", endpoint, **kwargs)

    def put(self, endpoint, **kwargs):
        return self._make_request("PUT", endpoint, **kwargs)

    def _make_request(self, method, endpoint, **kwargs):
        url = endpoint
        response = requests.request(method, url, **kwargs)
        response.requested_url = url
        response.requested_method = method

        return response





def check_connection(host, port):
    try:
        # Create a socket object
        with socket.create_connection((host, port), timeout=5) as sock:
            sock.close()
            return True
        
    except (socket.timeout, socket.error):
        print(f"Failed to connect to {host}:{port}")
        return False

###########################################################


req = CustomRequests(base_url)