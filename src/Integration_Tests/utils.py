import inspect
from api_utils import *

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