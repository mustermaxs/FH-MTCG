

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

firekraken = Card("", 10.0, "Ork", "fire", "monster")
firetroll = Card("", 10.0, "Wizard", "fire", "monster")
firespell = Card("", 10.0, "FireSpell", "fire", "monster")
waterspell = Card("", 10.0, "WaterSpell", "water", "spell")
regularspell = Card("", 10.0, "Knight", "normal", "spell")

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

# already in database
admin = User("admin", "admin", "admin bio", ":)", 100, "14076953-be7d-4bfb-9180-a797d9dad345", "33b039518718ba03b03e08e473032c9c4d72a2f9d5b27a43938c1bc49d4598a3")
normal_user = User("max", "max", "", ":)", 100, "060ab0c4-abef-4d89-bf45-c33af92a7c89", "7944947a9f442df12c947d07c71221d3ccf929fdc02837d74baec3e1b47dc1a1")
normal_user2 = User("test", "test", "", ":)", 100, "5eb99dbc-f562-4f9e-ba06-8b7b76d3dff2", "e622d2e010c5289b1e19f697180f094c4b1bcc220e5d40f4484b72052d18b8ba")
normal_user4 = User("toni", "toni", "", ":)", 100, "5eb99dbc-f562-4f9e-ba06-8b7b76d3dff2", "6f8b71631056cf8b60d7702b903a06fab6b3eda41b3cfba37b340cd9b89221da")

# for registration test
# not yet in database
normal_user3 = User("reginald", "reginald", "", ":)", 100, "", "")

users = {
    "admin": admin,
    "max": normal_user,
    "test": normal_user2,
    "registration_test_user": normal_user3,
    "toni": normal_user4
}