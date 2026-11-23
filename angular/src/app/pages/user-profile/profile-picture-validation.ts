const maxProfilePictureSize = 2 * 1024 * 1024;
const allowedTypes: Record<string, string> = {
  jpg: 'image/jpeg',
  jpeg: 'image/jpeg',
  png: 'image/png',
  webp: 'image/webp',
};

export function validateProfilePictureFile(file: File): string | null {
  const extension = file.name.toLowerCase().split('.').pop() ?? '';
  const expectedContentType = allowedTypes[extension];

  if (!expectedContentType || file.type.toLowerCase() !== expectedContentType) {
    return 'Elegí una imagen JPEG, PNG o WebP.';
  }

  if (file.size > maxProfilePictureSize) {
    return 'La imagen no puede superar los 2 MiB.';
  }

  return null;
}
