#!/bin/bash

is_minor="$1"    # type is "minor" or "patch"
is_patch="$2"    # type is "minor" or "patch"

echo "is_minor: $is_minor"
echo "is_patch: $is_patch"

if [ "$is_minor" = true ] && [ "$is_patch" = true ]; then
  echo "Only one of minor version and patch version should be specified."
  exit 1
elif [ "$is_minor" = true ] && [ "$is_patch" = false ]; then
  type="minor"
  echo "decided to update minor version"
elif [ "$is_minor" = false ] && [ "$is_patch" = true ]; then
  type="patch"
  echo "decided to update patch version"
else
  echo "At least one of minor version and patch version should be specified."
  exit 1
fi

mkdir ./temp;
mkdir ./temp/runtimes;
cp ./LLama/runtimes/*.* ./temp/runtimes/;
cp ./LLama/runtimes/build/*.* ./temp/;

# get the current version
cd temp;
dotnet add package LLamaSharp;
version=$(dotnet list temp.csproj package | grep LLamaSharp);
read -ra arr <<< "$version"
version="${arr[-1]}"
echo "The latest version: $version";


# update the version
if [[ $type == "minor" ]]; then
    regex="[0-9]+\.([0-9]+)\.[0-9]+$"
    if [[ $version =~ $regex ]]; then
        b="${BASH_REMATCH[1]}"
        b=$((b + 1))
        updated_version="${version%%.*}.$b.0"
        echo "Updated version: $updated_version"
    else
        echo "Invalid version format" exit 1
    fi
elif [[ $type == "patch" ]]; then
    regex="([0-9]+)$"
    if [[ $version =~ $regex ]]; then
        c="${BASH_REMATCH[1]}"
        c=$((c + 1))
        updated_version="${version%.*}.$c"
        echo "Updated version: $updated_version"
    else
        echo "Invalid version format"
        exit 1
    fi
else
    echo "Invalid type"
    exit 1
fi

cd ..
# pack the main package
dotnet pack ./LLama/LLamaSharp.csproj -c Release -o ./temp/ /p:PackageVersion=$updated_version /p:Version=$updated_version;
dotnet pack ./LLama.SemanticKernel/LLamaSharp.SemanticKernel.csproj -c Release -o ./temp/ /p:PackageVersion=$updated_version /p:Version=$updated_version;

# pack the backends
cd temp
nuget pack LLamaSharp.Backend.Cpu.nuspec -version $updated_version
nuget pack LLamaSharp.Backend.Cuda11.nuspec -version $updated_version
nuget pack LLamaSharp.Backend.Cuda12.nuspec -version $updated_version
nuget pack LLamaSharp.Backend.MacMetal.nuspec -version $updated_version

cd ..
exit 0
