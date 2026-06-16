.PHONY: restore build test web-install web-build web-dev up down reset clean

restore:
	dotnet restore AgriSure.sln

build: restore
	dotnet build AgriSure.sln --configuration Release --no-restore

test: build
	dotnet test AgriSure.sln --configuration Release --no-build

web-install:
	cd src/Web/agrisure-web && npm ci

web-build: web-install
	cd src/Web/agrisure-web && npm run build

web-dev:
	cd src/Web/agrisure-web && npm run dev

up:
	docker compose up --build

down:
	docker compose down

reset:
	docker compose down --volumes

clean:
	dotnet clean AgriSure.sln
	rm -rf src/Web/agrisure-web/node_modules src/Web/agrisure-web/dist
