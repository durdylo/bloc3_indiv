# Stage 1: Build
FROM node:latest as build
WORKDIR /app

# Copier package.json et installer les dépendances
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

# Copier le reste des fichiers et construire l'application
COPY frontend/ ./
RUN npm run build -- --configuration production

# Stage 2: Servir l'application avec Nginx
FROM nginx:stable-alpine3.20-perl

COPY --from=build /app/dist/frontend /usr/share/nginx/html

# Configuration nginx pour Angular
COPY default.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]