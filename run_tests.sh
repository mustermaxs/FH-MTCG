#!/bin/bash

print_deco() {
    echo -e "###############################################"
}

if [ "$#" -eq 0 ]; then
    echo "[Error] Please provide an argument (integration, unit, both)"
    exit 1
fi

run_integration_tests() {
    print_deco
    echo -e "############   INTEGRATION TESTS   ############"
    print_deco
    find src/Integration_Tests -name "integration_tests.py" -exec python3 {} \;
}

run_unit_tests() {
    print_deco
    echo -e "############      UNIT TESTS       ############"
    print_deco
    cd MTCG.Tests && find . -name "*.csproj" -exec dotnet test {} \; && cd ..
}

while [[ "$#" -gt 0 ]]; do
    case $1 in
        -i|--integration)
            run_integration_tests
            ;;
        -u|--unit)
            run_unit_tests
            ;;
        -b|--both)
            run_integration_tests
            run_unit_tests
            ;;
        *)
            echo "[Error] Unknown option $1. Use -i for 'integration', -u for 'unit', or -b for 'both'."
            exit 1
            ;;
    esac
    shift
done


