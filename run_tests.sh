#!/bin/bash

src/Integration_Tests/integration_tests.py
cd MTCG.Tests && dotnet test && cd ..