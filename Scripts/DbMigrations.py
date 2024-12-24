import os
import subprocess
import ulid
from pathlib import Path

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
                print(f"Found 'DbContext.cs' files in {
                      context_dir} (excluding 'Contracts' and 'ReadOnlyDbContext.cs'):")
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
                print(f"Corresponding .csproj files: {
                      db_context_project_files}")
        else:
            print(f"No 'Contexts' directory found in: {dir_path}")

    # Loop through each DB context file name and run the migration command
    for db_context, project_file in zip(db_context_file_names, db_context_project_files):
        abs_project_file = Path(project_file).resolve()
        print(f"Resolved .csproj file: {abs_project_file}")

        # Build the project
        print(f"Building project: {abs_project_file}")
        build_result = subprocess.run(
            ["dotnet", "build", str(abs_project_file)], capture_output=True)

        if build_result.returncode != 0:
            print(f"Build failed for project: {abs_project_file}. Exiting...")
            print(build_result.stderr.decode())
            exit(1)

        # Generate a ULID
        ulid_str = str(ulid.new())
        print(f"Generated UUID: {ulid_str}")

        # Run the dotnet ef migrations add command
        migration_dir = f"Migrations/{db_context}"
        print(f"Running migration for DbContext: {db_context} with UUID: {
              ulid_str} and Project: {abs_project_file}, OUTPUT DIR: {migration_dir}")
        migration_result = subprocess.run([
            "dotnet", "ef", "migrations", "add", ulid_str,
            "--context", db_context,
            "--output-dir", migration_dir,
            "--project", str(abs_project_file)
        ], capture_output=True)

        if migration_result.returncode != 0:
            print(f"Migration failed for DbContext: {db_context}. Exiting...")
            print(migration_result.stderr.decode())
            exit(1)

print("All migrations completed successfully.")
