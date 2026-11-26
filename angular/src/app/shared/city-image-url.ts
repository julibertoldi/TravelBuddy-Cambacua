const LEGACY_GENERIC_CITY_IMAGE =
  'https://images.unsplash.com/photo-1500382017468-9049fed747ef?w=400';

export function getCityImageUrl(
  imageUrl: string | null | undefined,
  geoDbCityId: number | null | undefined,
): string {
  if (imageUrl && imageUrl !== LEGACY_GENERIC_CITY_IMAGE) {
    return imageUrl;
  }

  const cityId = typeof geoDbCityId === 'number' && Number.isSafeInteger(geoDbCityId)
    ? geoDbCityId
    : 0;
  const imageNumber = Math.abs(cityId % 6) + 1;

  return `/assets/images/generic-${imageNumber}.jpg`;
}
