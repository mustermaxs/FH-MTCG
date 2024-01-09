#!/bin/bash

echo "############   RUNNING TESTS   ############"

if [ "$#" -eq 0 ]; then
    echo "[Error] Please provide an argument (integration, unit, both)"
    exit 1
fi

run_integration_tests() {
    echo -e "\n\n############   INTEGRATION TESTS   ############\n\n"
    find src/Integration_Tests -name "integration_tests.py" -exec python3 {} \;
}

run_unit_tests() {
    echo -e "\n\n############      UNIT TESTS       ############\n\n"
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


