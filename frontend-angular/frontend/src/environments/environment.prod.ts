export const environment = {
  production: true,
  cameraServiceUrl: 'http://localhost',  // L'URL vide fait que les requêtes seront relatives 
  murImageServiceUrl: 'http://localhost' // au domaine courant et passeront par le proxy Nginx
};