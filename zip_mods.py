import os
import zipfile

def create_zip():
    source_dir = r"D:\SteamLibrary\steamapps\common\Slay the Spire 2\mods"
    zip_path = "mods.zip"

    print(f"正在从 {source_dir} 打包最新文件到 {zip_path}...")

    if os.path.exists(zip_path):
        os.remove(zip_path)

    with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_DEFLATED) as zipf:
        for folder in ["cakemod", "BaseLib"]:
            folder_path = os.path.join(source_dir, folder)
            if not os.path.exists(folder_path):
                print(f"警告: 找不到文件夹 {folder_path}")
                continue
                
            for root, dirs, files in os.walk(folder_path):
                for file in files:
                    file_path = os.path.join(root, file)
                    # 保留相对于 mods/ 的路径结构，即打包后里面有 cakemod/... 和 BaseLib/...
                    arcname = os.path.relpath(file_path, source_dir)
                    zipf.write(file_path, arcname)

    print("模组文件打包成功！")

if __name__ == "__main__":
    create_zip()
