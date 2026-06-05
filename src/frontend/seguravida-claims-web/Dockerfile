FROM node:22.13-alpine AS build
WORKDIR /app

COPY src/frontend/seguravida-claims-web/package*.json ./
RUN npm install --no-audit --no-fund --ignore-scripts

COPY src/frontend/seguravida-claims-web/ ./
RUN npm run build

FROM nginx:1.27-alpine
COPY src/frontend/seguravida-claims-web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/seguravida-claims-web/browser /usr/share/nginx/html
EXPOSE 80
