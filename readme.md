# Projekt: System Parkingowy

Aplikacja backendowa realizująca system zarządzania parkingiem miejskim, zaprojektowana w oparciu o zasady **Czystej Architektury (Clean Architecture)** z jawnym podziałem na warstwy AppCore, Infrastructure oraz WebApi.

## Repozytorium GitHub
Link do publicznego kodu źródłowego: https://github.com/kgwozdz01/parkowanie

## Technologie
* .NET 10.0
* ASP.NET Core Web API
* Entity Framework Core + SQLite (z automatycznymi migracjami)
* ASP.NET Core Identity + JWT Bearer Auth (z obsługą ról oraz Refresh Tokenów)

## Jak uruchomić lokalnie
1. Sklonuj repozytorium lub rozpakuj archiwum.
2. Otwórz solucję w środowisku JetBrains Rider lub Visual Studio.
3. Upewnij się, że projekt startowy to **`ParkingSystem.WebApi`**.
4. Uruchom projekt (aplikacja posiada wbudowany `DatabaseSeeder`, który przy pierwszym starcie automatycznie utworzy bazę SQLite, rolę administratora, konto admina, startowe taryfy oraz bramki).
5. Swagger otworzy się automatycznie pod adresem: `http://localhost:5270/swagger/`

## Testowanie API
W projekcie WebApi znajduje się plik `test.http` przeznaczony do szybkiego i wygodnego testowania endpointów bezpośrednio z poziomu edytora (Rider / VS).

### Dane logowania administratora (Seeder):
* **Email:** `admin@parkowanko.pl`
* **Hasło:** `Admin123!`

### Przegląd funkcjonalności i endpointów:
* **Moduł Administratora (`AdminController` - zabezpieczony rolą Admin):** Umożliwia dynamiczne zarządzanie taryfami (tworzenie taryf standardowych, określanie stawek, darmowych minut i limitów dobowych oraz ich aktywację) oraz rejestrację i zmianę statusu operacyjnego bramek/szlabanów.
* **Moduł Kierowcy (`DriverController` - publiczny):** Obsługuje symulację ruchu na parkingu – generowanie unikalnych biletów (`Ticket`) z rejestracją czasu przy wjeździe (`/api/driver/enter`) oraz automatyczne kalkulowanie opłat przy wyjeździe (`/api/driver/calculate-fee`) na podstawie aktywnej taryfy (z uwzględnieniem czasu darmowego, zaokrąglania do pełnych godzin oraz limitu dobowego).