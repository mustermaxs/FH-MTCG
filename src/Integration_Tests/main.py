import requests
import unittest
import json
import pickle
import inspect
from testcases import *
import random
import asyncio



async def main():
    # SETUP
    reset()


    # test_user = User("test", "test", "", ":)", 100, "", "")
    await test_battle()
    # await test_battle_multiple_clients()
    test_register_alreadyexisting_user()
    test_retrieve_packages_no_packages()
    test_login(users["max"])
    test_user_cards_in_deck_true()
    test_create_package()
    packages = test_retrieve_packages_has_packages()

    if packages is not None:
        test_delete_package(packages[0]["Id"])

    test_user_cards_in_deck_true()
    test_get_all_users()
    test_user_no_cards_in_stack_true()
    test_add_trading_deal()
    test_add_card_to_stack()
    test_accept_cardtrade_deal()
    test_aquire_package()
    test_logout_user()
    # test_register_user()
    test_update_user()