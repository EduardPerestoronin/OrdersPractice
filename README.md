# OrdersPractice

[![CI](https://github.com/EduardPerestoronin/OrdersPractice/actions/workflows/ci.yml/badge.svg)](https://github.com/EduardPerestoronin/OrdersPractice/actions/workflows/ci.yml)

Учебное приложение «Заказы»: unit-тесты на C# с использованием xUnit и Moq.

## Структура

- `src/Orders` — тестируемый код (заказы, скидки, сервис, обработчик сообщений).
- `tests/Orders.Tests` — unit-тесты (37 запусков, покрытие классов 100%).
- `.github/workflows/ci.yml` — автоматическая сборка и запуск тестов в GitHub Actions.

## Запуск

dotnet restore
dotnet build
dotnet test