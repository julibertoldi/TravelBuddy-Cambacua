import { getCityImageUrl } from './city-image-url';

describe('getCityImageUrl', () => {
  it('selects a deterministic generic image from the GeoDB city id', () => {
    expect(getCityImageUrl(undefined, 0)).toBe('/assets/images/generic-1.jpg');
    expect(getCityImageUrl(undefined, 5)).toBe('/assets/images/generic-6.jpg');
    expect(getCityImageUrl(undefined, 6)).toBe('/assets/images/generic-1.jpg');
  });

  it('replaces the legacy generic URL with the city-specific image', () => {
    expect(getCityImageUrl(
      'https://images.unsplash.com/photo-1500382017468-9049fed747ef?w=400',
      2,
    )).toBe('/assets/images/generic-3.jpg');
  });

  it('preserves a representative image URL', () => {
    expect(getCityImageUrl('https://upload.wikimedia.org/city.jpg', 2))
      .toBe('https://upload.wikimedia.org/city.jpg');
  });
});
