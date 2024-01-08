#!/usr/bin/env python3
import requests
import unittest
import json
import pickle
import inspect
from testcases import *




def main():
    # SETUP
    reset()

    test_user = User("test", "test", "", ":)", 100, "", "")

    test_register_alreadyexisting_user(test_user)
    test_retrieve_packages_no_packages()
    test_login(users["max"])
    test_create_package(cards, users["admin"])
    packages = test_retrieve_packages_has_packages()

    if packages is not None:
        test_delete_package(packages[0]["Id"])
    test_get_all_users()
    test_user_no_cards_in_stack_true(users["max"])
    test_aquire_package_and_create_deck(users["max"], cards)
    test_add_trading_deal(users["max"])

if __name__ == "__main__":
    main()