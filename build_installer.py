import os
import subprocess

def get_version():
    last_version = "v0.0.0"
    try:
        with open("更新.txt", "r", encoding="utf-8") as f:
            for line in f:
                line = line.strip()
                if line.lower().startswith('v'):
                    last_version = line
    except Exception as e:
        print("无法读取 更新.txt:", e)
    return last_version

def main():
    version = get_version()
    # 确保版本号中不包含非法文件名字符（例如：空格、<>:"/\|?* 等）
    version = version.replace(" ", "").replace("/", "-").replace("\\", "-")
    
    # 文件名不加加号
    app_name = f"蛋糕尖塔{version}"
    print(f"检测到版本号: {version}")
    print(f"即将生成安装包: {app_name}.exe")
    
    command = [
        "python", "-m", "PyInstaller",
        "--onefile",
        "--add-data", "mods.zip;.",
        "--add-data", "更新.txt;.",
        "--name", app_name,
        "install.py"
    ]
    
    # 打包前清理旧文件
    dist_dir = "dist"
    if os.path.exists(dist_dir):
        for f in os.listdir(dist_dir):
            if f.endswith(".exe"):
                file_path = os.path.join(dist_dir, f)
                try:
                    os.remove(file_path)
                    print(f"已清理旧安装包: {f}")
                except Exception as e:
                    print(f"无法删除旧安装包 {f}: {e}")
    
    result = subprocess.run(command)
    
    if result.returncode == 0:
        print(f"\n打包成功！安装包已在 dist 文件夹中生成: {app_name}.exe")
    else:
        print("\n打包失败，请检查错误输出。")

    # 清理产生的 spec 文件
    spec_file = f"{app_name}.spec"
    if os.path.exists(spec_file):
        try:
            os.remove(spec_file)
            print(f"已清理无用的配置: {spec_file}")
        except Exception as e:
            print(f"无法删除 {spec_file}: {e}")

if __name__ == "__main__":
    main()
