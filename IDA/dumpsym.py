# autopep8: off

import dumpsym_function_prototypes
import dumpsym_names
import importlib

importlib.reload(dumpsym_function_prototypes)
importlib.reload(dumpsym_names)

from dumpsym_function_prototypes import dumpsym_function_prototypes
from dumpsym_names import dumpsym_names

# autopep8: on


def dumpsym_initialize():
    import ida_kernwin
    import datetime
    print(datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S.%f"))
    ida_kernwin.process_ui_action("OutputClearContents")


def dumpsym_cleanup():
    import ida_kernwin
    print("Refreshing decompilation...")
    ida_kernwin.process_ui_action("hx:GenPseudo")


def dumpsym_apply_function_prototype(addr: int, decl: str) -> bool:
    import ida_typeinf

    if not ida_typeinf.apply_cdecl(None, addr, decl):
        print(f"Failed to set function prototype declaration.")
        return False

    return True


def dumpsym_apply_function_prototypes(prototypes):
    print(f"Applying {len(prototypes)} function prototypes...")
    for addr, name, decl in prototypes:
        print(f"Applying function prototype to {hex(addr)}")
        if not dumpsym_apply_function_prototype(addr, decl):
            print("Applying function prototype failed, aborting.")
            break


def dumpsym_apply_names(names):
    import idc
    import ida_name
    print(f"Applying {len(names)} names...")
    for addr, name in names:
        print(f"Applying name to {hex(addr)}: {name}")
        if not idc.set_name(addr, name, ida_name.SN_DELTAIL | ida_name.SN_FORCE):
            print(f"Failed to set name {name} at {hex(addr)}")
            break


dumpsym_initialize()

dumpsym_apply_function_prototypes(dumpsym_function_prototypes)

dumpsym_apply_names(dumpsym_names)

dumpsym_cleanup()
