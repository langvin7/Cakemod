import os
import sys
import winreg
import zipfile
import shutil

def get_steam_path():
    try:
        # Try finding Steam path from registry
        key = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\WOW6432Node\Valve\Steam")
        steam_path, _ = winreg.QueryValueEx(key, "InstallPath")
        return steam_path
    except WindowsError:
        try:
            key = winreg.OpenKey(winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\Valve\Steam")
            steam_path, _ = winreg.QueryValueEx(key, "InstallPath")
            return steam_path
        except WindowsError:
            return None

def get_library_folders(steam_path):
    vdf_path = os.path.join(steam_path, "steamapps", "libraryfolders.vdf")
    libraries = [steam_path]
    
    if os.path.exists(vdf_path):
        with open(vdf_path, 'r', encoding='utf-8') as f:
            for line in f:
                if '"path"' in line:
                    path = line.split('"')[3]
                    # Convert double backslashes to single ones
                    path = path.replace('\\\\', '\\')
                    if path not in libraries:
                        libraries.append(path)
    return libraries

def is_valid_game_path(path):
    if not os.path.exists(path):
        return False
    try:
        # 检查文件夹内是否包含游戏本体文件（如.exe或.pck）
        for f in os.listdir(path):
            if f.lower().endswith('.exe') or f.lower().endswith('.pck'):
                return True
    except Exception:
        pass
    return False

def find_sts2_path():
    steam_path = get_steam_path()
    if not steam_path:
        print("未找到Steam安装路径。")
        return None

    libraries = get_library_folders(steam_path)
    
    for lib in libraries:
        game_path = os.path.join(lib, "steamapps", "common", "Slay the Spire 2")
        if is_valid_game_path(game_path):
            return game_path
            
    print("未在Steam库中找到 Slay the Spire 2。")
    return None

def uninstall_mods(mods_path):
    print("\n准备卸载模组...")
    cakemod_path = os.path.join(mods_path, "cakemod")
    baselib_path = os.path.join(mods_path, "BaseLib")
    
    uninstalled_any = False
    
    if os.path.exists(cakemod_path):
        try:
            shutil.rmtree(cakemod_path)
            print("成功删除 cakemod。")
            uninstalled_any = True
        except Exception as e:
            print(f"删除 cakemod 时出错: {e}")
            
    if os.path.exists(baselib_path):
        # 考虑到 BaseLib 可能被其他模组依赖，询问是否删除
        choice = input("检测到 BaseLib (这可能被其他模组依赖)，是否一并删除？[y/N]: ").strip().lower()
        if choice == 'y':
            try:
                shutil.rmtree(baselib_path)
                print("成功删除 BaseLib。")
                uninstalled_any = True
            except Exception as e:
                print(f"删除 BaseLib 时出错: {e}")
    
    if not uninstalled_any:
        print("未检测到已安装的 cakemod 模组。")
    else:
        print("卸载完成！")

def install_mods(mods_path):
    # Create mods folder if it doesn't exist
    if not os.path.exists(mods_path):
        print(f"正在创建 mods 文件夹...")
        os.makedirs(mods_path)
        
    print(f"安装目标路径: {mods_path}")
    
    # Locate self/zip resource
    # The zip is bundled with PyInstaller
    bundle_dir = getattr(sys, '_MEIPASS', os.path.abspath(os.path.dirname(__file__)))
    zip_path = os.path.join(bundle_dir, 'mods.zip')
    
    if not os.path.exists(zip_path):
        print(f"错误：找不到资源文件 {zip_path}")
        return False
        
    print("正在解压模组文件...")
    try:
        with zipfile.ZipFile(zip_path, 'r') as zip_ref:
            baselib_exists = os.path.exists(os.path.join(mods_path, "BaseLib"))
            skipped_baselib = False
            
            for member in zip_ref.infolist():
                # 如果检测到是 BaseLib 的文件，并且对方电脑已经安装了 BaseLib，则跳过覆盖
                if member.filename.startswith("BaseLib/") or member.filename.startswith("BaseLib\\"):
                    if baselib_exists:
                        skipped_baselib = True
                        continue
                
                zip_ref.extract(member, mods_path)
                
        print("\n安装成功！")
        if skipped_baselib:
            print("已安装/更新模组: cakemod")
            print("(检测到已有 BaseLib，为了防止版本冲突，本次未覆盖对方的 BaseLib)")
        else:
            print("已安装/更新模组: cakemod, BaseLib")
        return True
    except Exception as e:
        print(f"\n安装过程中出现错误: {e}")
        return False

def get_update_info():
    bundle_dir = getattr(sys, '_MEIPASS', os.path.abspath(os.path.dirname(__file__)))
    update_txt_path = os.path.join(bundle_dir, '更新.txt')
    all_text = ""
    latest_text = ""
    if os.path.exists(update_txt_path):
        try:
            with open(update_txt_path, 'r', encoding='utf-8') as f:
                lines = f.readlines()
                all_text = "".join(lines)
                
                # 寻找最后一个以 'v' 开头的版本号所在行
                last_v_index = -1
                for i in range(len(lines)):
                    if lines[i].strip().lower().startswith('v'):
                        last_v_index = i
                
                if last_v_index != -1:
                    latest_text = "".join(lines[last_v_index:])
                else:
                    latest_text = all_text
        except Exception as e:
            pass
    return latest_text.strip(), all_text.strip()

def main():
    latest_text, all_text = get_update_info()
    
    while True:
        print("\n==========================================")
        print("  Slay the Spire 2 模组安装器 (cakemod)  ")
        print("==========================================")
        
        if latest_text:
            print("\n【最新更新说明】")
            print(latest_text)
            print("==========================================")
        
        print("\n请选择要执行的操作:")
        print("  [1] 安装 / 更新 模组")
        print("  [2] 卸载 模组")
        print("  [3] 阅读全部历史更新文本")
        
        choice = input("请输入 1, 2 或 3，然后按回车: ").strip()
        
        if choice == '3':
            print("\n================ 全部历史更新 ================")
            print(all_text)
            print("==========================================")
            input("\n按回车键返回菜单...")
            continue
        elif choice in ['1', '2']:
            break
        else:
            print("无效输入，请重新输入。")
            
    print("-" * 50)
    
    game_path = find_sts2_path()
    
    if not game_path:
        # Fallback manual entry
        game_path = input("自动检测失败，请输入 Slay the Spire 2 的安装路径\n(例如 D:\\SteamLibrary\\steamapps\\common\\Slay the Spire 2): ").strip()
        if not os.path.exists(game_path):
            print("错误：提供的路径不存在！")
            input("按回车键退出...")
            sys.exit(1)
            
    print(f"找到游戏路径: {game_path}")
    mods_path = os.path.join(game_path, "mods")
    
    if choice == '1':
        install_mods(mods_path)
    elif choice == '2':
        uninstall_mods(mods_path)
        
    print("-" * 50)
    input("按回车键退出...")

if __name__ == "__main__":
    main()
