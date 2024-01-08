

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