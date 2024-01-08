import requests

base_url = "http://localhost:12000" 
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
        "cards_all": "/cards"
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

###########################################################


req = CustomRequests(base_url)