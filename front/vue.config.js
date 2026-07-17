const { defineConfig } = require('@vue/cli-service')
module.exports = defineConfig({
  transpileDependencies: true,
  devServer: {
    proxy: {
      '/api/auth': {
        target: 'http://localhost:5253',
        changeOrigin: true,
        pathRewrite: { '^/api/auth': '/auth' }
      },
      '/api/power': {
        target: 'http://localhost:5169',
        changeOrigin: true,
        pathRewrite: { '^/api/power': '' }
      },
      '/api/inference': {
        target: 'http://localhost:8000',
        changeOrigin: true,
        pathRewrite: { '^/api/inference': '' }
      }
    }
  }
})
