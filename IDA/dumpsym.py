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


def dumpsym_init_hexrays() -> bool:
    import ida_hexrays  # pyright: ignore[reportMissingModuleSource]

    return ida_hexrays.init_hexrays_plugin()


def dumpsym_refresh_ui() -> None:
    import ida_kernwin  # pyright: ignore[reportMissingModuleSource]

    ida_kernwin.refresh_idaview_anyway()


def dumpsym_rename_func_regs(ea: int, regs: list[tuple[int, str]]) -> None:
    import ida_funcs  # pyright: ignore[reportMissingModuleSource]
    import ida_hexrays  # pyright: ignore[reportMissingModuleSource]
    import ida_idp  # pyright: ignore[reportMissingModuleSource]

    func = ida_funcs.get_func(ea)
    if not func:
        raise RuntimeError("Couldn't get function at $%08X." % ea)

    code = ida_hexrays.decompile(ea)
    if not code:
        raise RuntimeError("Couldn't decompile function at %08X." % ea)

    old_names = ida_idp.ph_get_regnames()
    new_names = {}

    for reg_slot, reg_name in regs:
        new_name = old_names[reg_slot]
        new_names[new_name] = reg_name

    lvars = code.get_lvars()
    for lvar in lvars:
        loc = ida_hexrays.print_vdloc(lvar.location, lvar.width)
        if not loc in new_names:
            continue

        old_name = lvar.name
        new_name = new_names[loc]
        if not ida_hexrays.rename_lvar(ea, old_name, new_name):
            raise RuntimeError("Couldn't rename %s to %s." % (old_name, new_name))

        print(
            "Renamed $%08X register '%s' from '%s' to '%s'."
            % (ea, loc, old_name, new_name)
        )

def main():
    dumpsym_initialize()

    dumpsym_apply_function_prototypes(dumpsym_function_prototypes)

    dumpsym_apply_names(dumpsym_names)

    dumpsym_cleanup()

if __name__ == "__main__":
    main()
