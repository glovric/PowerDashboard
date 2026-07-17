# Build the Vue app
FROM node:22-alpine AS frontend-build

WORKDIR /app

COPY front/package*.json ./
RUN npm install

COPY front/ .
RUN npm run build

# Nginx image
FROM nginx:alpine

#COPY nginx/conf.d /etc/nginx/conf.d
COPY --from=frontend-build /app/dist /usr/share/nginx/html