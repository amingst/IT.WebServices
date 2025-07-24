#!/bin/bash

# TypeScript generation script for all IT proto files

# Ensure we're in the correct directory (IT.Fragments)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Set up PATH for protoc plugins
export PATH="$PATH:$PWD/node_modules/.bin"

echo "🚀 Starting TypeScript generation for all proto files..."
echo "📍 Working directory: $(pwd)"
echo "🔧 PATH includes: $PWD/node_modules/.bin"

# Clean existing generated files
echo "🧹 Cleaning existing generated files..."
rm -rf ts-gen/gen/*
rm -rf ts-gen/gen
mkdir -p ts-gen/gen
echo "  ✓ Removed all previous generated files"

# Discover available modules by scanning the Protos directory
echo "🔍 Discovering available proto modules..."
modules=()
for dir in Protos/IT/WebServices/Fragments/*/; do
    if [ -d "$dir" ]; then
        module_name=$(basename "$dir")
        # Check if directory contains .proto files
        if find "$dir" -name "*.proto" -type f | head -1 | grep -q .; then
            modules+=("$module_name")
            echo "  ✓ Found module: $module_name"
        fi
    fi
done

# Also check for proto files directly in the Fragments directory
if find "Protos/IT/WebServices/Fragments/" -maxdepth 1 -name "*.proto" -type f | head -1 | grep -q .; then
    echo "  ✓ Found proto files in root Fragments directory"
fi

echo "📦 Generating modules: ${modules[*]}"
failed_modules=()
successful_modules=()

for module in "${modules[@]}"; do
    echo "  → Generating $module..."
    buf generate --path "Protos/IT/WebServices/Fragments/$module"
    if [ $? -eq 0 ]; then
        echo "  ✓ $module generated successfully"
        successful_modules+=("$module")
    else
        echo "  ✗ Failed to generate $module"
        failed_modules+=("$module")
    fi
done

# Generate root-level proto files if any exist
if find "Protos/IT/WebServices/Fragments/" -maxdepth 1 -name "*.proto" -type f | head -1 | grep -q .; then
    echo "  → Generating root-level proto files..."
    buf generate --path "Protos/IT/WebServices/Fragments/"
    if [ $? -eq 0 ]; then
        echo "  ✓ Root-level files generated successfully"
    else
        echo "  ✗ Failed to generate root-level files"
    fi
fi

# Report results
echo ""
echo "📊 Generation Summary:"
echo "  ✅ Successful modules (${#successful_modules[@]}): ${successful_modules[*]}"
if [ ${#failed_modules[@]} -gt 0 ]; then
    echo "  ❌ Failed modules (${#failed_modules[@]}): ${failed_modules[*]}"
    echo "  ⚠️  Note: Failed modules may have protobuf definition conflicts that need to be resolved"
fi

# Fix any potential import path issues in generated files
echo "🔧 Fixing import path issues..."
find ts-gen/gen -name "*.ts" -type f -exec sed -i 's|\\|/|g' {} \;
echo "  ✓ Import path fixes applied"

# Count generated files
total_files=$(find ts-gen/gen -name "*.ts" -type f | wc -l)
echo "🎉 Generation complete! Generated $total_files TypeScript files."

# List generated modules
echo "📁 Generated modules:"
find ts-gen/gen -type d -name "*" | grep -v "^ts-gen/gen$" | sort | sed 's|ts-gen/gen/||g' | sed 's|^|  - |g'

# Dynamic index.ts generation - hierarchical approach
echo ""
echo "📝 Building hierarchical index.ts files..."

# Function to generate index.ts for a directory
generate_directory_index() {
    local dir_path="$1"
    local index_file="$dir_path/index.ts"
    
    echo "  → Generating index for: $dir_path"
    
    # Create header
    cat > "$index_file" << EOF
// Auto-generated index file - DO NOT EDIT MANUALLY
// Generated on: $(date)

EOF
    
    # Export all TypeScript files in current directory
    if find "$dir_path" -maxdepth 1 -name "*.ts" -not -name "index.ts" -type f | head -1 | grep -q .; then
        echo "// Direct exports from this module" >> "$index_file"
        find "$dir_path" -maxdepth 1 -name "*.ts" -not -name "index.ts" -type f | sort | while read -r file; do
            filename=$(basename "$file" .ts)
            echo "export * from './$filename';" >> "$index_file"
        done
        echo "" >> "$index_file"
    fi
    
    # Export subdirectories that have TypeScript files
    if find "$dir_path" -mindepth 1 -maxdepth 1 -type d | head -1 | grep -q .; then
        echo "// Re-exports from subdirectories" >> "$index_file"
        find "$dir_path" -mindepth 1 -maxdepth 1 -type d | sort | while read -r subdir; do
            subdir_name=$(basename "$subdir")
            # Check if subdirectory has TypeScript files (recursively)
            if find "$subdir" -name "*.ts" -type f | head -1 | grep -q .; then
                echo "export * as $subdir_name from './$subdir_name';" >> "$index_file"
            fi
        done
    fi
}

# Generate index files for all directories containing TypeScript files
# Start from the deepest level and work up
echo "🔍 Finding all directories with TypeScript files..."

# Find all directories that contain .ts files and sort by depth (deepest first)
directories_with_ts=$(find ts-gen/gen -type f -name "*.ts" -exec dirname {} \; | sort -u | sort -r)

echo "$directories_with_ts" | while read -r dir; do
    generate_directory_index "$dir"
done

# Also generate index files for intermediate directories that don't have direct .ts files
# but have subdirectories with .ts files
echo "🔍 Generating index files for intermediate directories..."
all_directories=$(find ts-gen/gen -type d | grep -v "^ts-gen/gen$" | sort -r)

echo "$all_directories" | while read -r dir; do
    # Skip if index already exists
    if [ ! -f "$dir/index.ts" ]; then
        # Check if this directory has subdirectories with TypeScript files
        if find "$dir" -mindepth 1 -name "*.ts" -type f | head -1 | grep -q .; then
            generate_directory_index "$dir"
        fi
    fi
done

# Generate main index.ts in ts-gen directory
MAIN_INDEX_FILE="ts-gen/index.ts"
echo "📝 Building main index.ts file..."

cat > "$MAIN_INDEX_FILE" << 'EOF'
// Auto-generated main index file - DO NOT EDIT MANUALLY
// This file provides access to all generated protobuf definitions
// Generated on: DATE_PLACEHOLDER

// Export everything from the generated protos
export * from './gen/Protos';

EOF

# Replace date placeholder
sed -i "s/DATE_PLACEHOLDER/$(date)/" "$MAIN_INDEX_FILE"

echo "  ✓ Hierarchical index.ts files created successfully!"
echo "  📍 Main index location: $MAIN_INDEX_FILE"

# Count total index files created
total_index_files=$(find ts-gen/gen -name "index.ts" -type f | wc -l)
echo "  📊 Generated $total_index_files hierarchical index.ts files"

# Show summary
echo ""
echo "📊 Generation Summary:"
total_ts_files=$(find ts-gen/gen -name "*.ts" -not -name "index.ts" -type f | wc -l)
echo "  - TypeScript files: $total_ts_files"
echo "  - Index files: $total_index_files"
echo "  - Total files: $((total_ts_files + total_index_files))"

echo ""
echo "✅ TypeScript generation and index building complete!"