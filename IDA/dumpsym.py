from dumpsym_function_prototypes import dumpsym_function_prototypes


def dumpsym_initialize():
    import ida_kernwin
    import datetime
    print(datetime.datetime.now().strftime("%Y-%m-%d %H:%M:%S.%f"))
    ida_kernwin.process_ui_action("OutputClearContents")


def dumpsym_cleanup():
    import ida_kernwin
    print("Refreshing decompilation...")
    ida_kernwin.process_ui_action("hx:GenPseudo")


def dumpsym_apply_function_prototype(addr: int, name: str, decl: str) -> bool:
    import ida_typeinf
    import idc

    if not ida_typeinf.apply_cdecl(None, addr, decl):
        print(f"Failed to set function prototype declaration.")
        return False

    if not idc.set_name(addr, name):
        print(f"Failed to set function prototype name.")
        return False

    return True


def dumpsym_apply_function_prototypes(prototypes):
    print(f"Applying {len(prototypes)} function prototypes...")
    for addr, name, decl in prototypes:
        print(f"Applying function prototype to {hex(addr)}")
        if not dumpsym_apply_function_prototype(addr, name, decl):
            print("Applying function prototype failed, aborting.")
            break


dumpsym_initialize()

dumpsym_apply_function_prototypes(dumpsym_function_prototypes)

dumpsym_cleanup()
