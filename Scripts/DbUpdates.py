import os
from pathlib import Path
import subprocess

# Set the parent directory (current directory in this case)
PARENT_DIR = Path.cwd().parent

# Find all directories ending with '.Infrastructure', excluding 'BuildingBlocks.Infrastructure'
infra_dirs = [
    dir_path for dir_path in PARENT_DIR.rglob("*.Infrastructure")
    if dir_path.is_dir() and "BuildingBlocks.Infrastructure" not in str(dir_path)
]

if not infra_dirs:
    print("No directories found.")
else:
    print("Directories ending with '.Infrastructure' (excluding 'BuildingBlocks.Infrastructure'):")
    for dir_path in infra_dirs:
        print(dir_path)

    db_context_file_names = []
    db_context_project_files = []

    # Iterate over each directory to look for 'Contexts' subdirectory
    for dir_path in infra_dirs:
        context_dir = dir_path / "Contexts"

        if context_dir.exists():
            print(f"Found 'Contexts' directory in: {dir_path}")

            # Search for files ending with 'DbContext.cs', excluding 'Contracts' and 'ReadOnlyDbContext.cs'
            db_context_files = [
                file for file in context_dir.rglob("*DbContext.cs")
                if "Contracts" not in str(file) and not file.name.endswith("ReadOnlyDbContext.cs")
            ]

            if not db_context_files:
                print(f"No 'DbContext.cs' files found in {context_dir}")
            else:
                print(f"Found 'DbContext.cs' files in {context_dir} (excluding 'Contracts' and 'ReadOnlyDbContext.cs'):")
                for file in db_context_files:
                    print(file)

                    # Extract the file name without path and extension
                    file_name = file.stem
                    db_context_file_names.append(file_name)

                    # Find the corresponding .csproj file
                    project_file = next(dir_path.rglob("*.csproj"), None)
                    if project_file:
                        db_context_project_files.append(str(project_file))

                print(f"DB context file names: {db_context_file_names}")
                print(f"Corresponding .csproj files: {db_context_project_files}")
        else:
            print(f"No 'Contexts' directory found in: {dir_path}")

    # Loop through each DB context file name and run the database update command
    for db_context, project_file in zip(db_context_file_names, db_context_project_files):
        abs_project_file = Path(project_file).resolve()
        print(f"Resolved .csproj file: {abs_project_file}")

        # Apply the most recent migration
        print(f"Applying the most recent migration for DbContext: {db_context}")
        update_result = subprocess.run([
            "dotnet", "ef", "database", "update",
            "--context", db_context,
            "--project", str(abs_project_file)
        ], capture_output=True)

        if update_result.returncode != 0:
            print(f"Failed to apply migration for DbContext: {db_context}.")
            print(update_result.stderr.decode())
        else:
            print(f"Migration applied successfully for DbContext: {db_context}.")
