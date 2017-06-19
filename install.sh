# /bin/bash
# install.sh

install=true
reset=true

while [[ $# > 0 ]]; do
    lowerI="$(echo $1 | awk '{print tolower($0)}')"
    case $lowerI in
        -r|--reset)
            reset=true
            ;;
        -i|--install)
            ;;
        -h|--help)
            echo "Options:"
            echo "  -r|--reset      Removes all user-defined templates."
            echo "  -i|--install    Install all user-defined templates."
            echo "  -h|--help       Display all commands."
            exit 0
            ;;
        *)
            break
            ;;
    esac

    shift
done

echo ""
echo "===Prerequisites==="

if [ ! -d /usr/local/share/dotnet/sdk ]; then
	echo "Hold up..."
    echo "  dotnet isn't installed, you must install dotnet core to use dotnet new command."
    echo "  Go do that and try again."
    exit 0
fi

echo "Found dotnet cli at /usr/local/share/dotnet."
echo ""

root=$PWD

echo ""

# --reset

if [ "$reset" = true ]; then

	echo "===Reset Installed Template==="

	dotnet new --debug:reinit
	
	echo "Removed user-defined templates."
	echo ""
fi

## --install

if [ "$install" = true ]; then
echo "===Installing Forms Templates==="
echo ""

echo "Installing Blank Forms App"
echo ""
dotnet new --install "$root/multiplatform/forms/blank"
echo ""
echo "Installed Blank Forms App."
echo ""

fi