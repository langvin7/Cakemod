import os

scripts_dir = "Scripts"
renamed_count = 0

# 需要移除的后缀列表
suffixes_to_remove = [
    "ConstructorPatch",
    "CanonicalVarsPatch"
]

for root, dirs, files in os.walk(scripts_dir):
    for file in files:
        if file.endswith(".cs"):
            new_filename = file
            for suffix in suffixes_to_remove:
                new_filename = new_filename.replace(suffix, "")
            
            if file != new_filename:
                filepath = os.path.join(root, file)
                new_filepath = os.path.join(root, new_filename)
                
                print(f"重命名 {file} -> {new_filename}")
                
                # Windows 大小写不敏感，如果只是大小写变化（本例不是），需要中间重命名
                if os.path.exists(new_filepath) and file.lower() != new_filename.lower():
                    print(f"警告: {new_filepath} 已存在，尝试直接覆盖或跳过...")
                
                try:
                    os.rename(filepath, new_filepath)
                except Exception as e:
                    print(f"重命名 {file} 失败: {e}")
                    continue
                
                # 对应的 .uid 文件也要重命名 (Godot 的需要)
                uid_filepath = filepath + ".uid"
                if os.path.exists(uid_filepath):
                    new_uid_filepath = new_filepath + ".uid"
                    try:
                        os.rename(uid_filepath, new_uid_filepath)
                    except Exception as e:
                        print(f"重命名 UID {uid_filepath} 失败: {e}")
                
                renamed_count += 1

print(f"总计成功去除了 {renamed_count} 个文件的后缀。")
