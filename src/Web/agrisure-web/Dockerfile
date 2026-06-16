FROM node:22-alpine AS build
WORKDIR /app
COPY src/Web/agrisure-web/package*.json ./
RUN npm ci
COPY src/Web/agrisure-web/ ./
RUN npm run build

FROM nginx:1.29-alpine
COPY src/Web/agrisure-web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
