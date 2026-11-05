using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace SistemadeRenta.Infrastructure.Services
{
    public class ServicioImagenes
    {
        private readonly Cloudinary _cloudinary;

        public ServicioImagenes(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<(string Url, string IdPublico)> SubirAsync(
            Stream archivo,
            string nombreArchivo)
        {
            var parametros = new ImageUploadParams
            {
                File = new FileDescription(nombreArchivo, archivo),
                PublicId = $"vehiculos/{Guid.NewGuid():N}"
            };

            var resultado = await _cloudinary.UploadAsync(parametros);

            if (resultado.Error != null ||
                resultado.SecureUrl == null ||
                string.IsNullOrWhiteSpace(resultado.PublicId))
            {
                throw new InvalidOperationException(
                    "No se pudo subir la foto a Cloudinary.");
            }

            return (
                resultado.SecureUrl.ToString(),
                resultado.PublicId
            );
        }

        public async Task EliminarAsync(string idPublico)
        {
            if (string.IsNullOrWhiteSpace(idPublico))
            {
                return;
            }

            var resultado = await _cloudinary.DestroyAsync(
                new DeletionParams(idPublico)
                {
                    Invalidate = true
                });

            if (resultado.Error != null ||
                (resultado.Result != "ok" &&
                 resultado.Result != "not found"))
            {
                throw new InvalidOperationException(
                    "No se pudo eliminar la foto de Cloudinary.");
            }
        }
    }
}
