FROM node:22-alpine
WORKDIR /app
COPY package.json package-lock.json* ./
RUN npm install --omit=dev --no-audit --no-fund || npm install --omit=dev --no-audit --no-fund --legacy-peer-deps
COPY . .
EXPOSE 8765
ENV PORT=8765
CMD ["node", "server.mjs"]
