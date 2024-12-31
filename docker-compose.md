# Docker Compose Commands

## Start services with a specific environment file and build the images

## For Development

```sh
docker compose --env-file .env.development up -d --build
```

## For Staging

```sh
docker compose --env-file .env.staging up -d --build
```

## For Production

```sh
docker compose --env-file .env.production up -d --build
```
