
export interface FavoriteDto {
  destinoId?: string;
  name?: string;
  description?: string;
  region?: string;
  country?: string;
  price: number;
  imageUrl?: string;
  isAvailable: boolean;
  geoDbCityId?: number;
  population: number;
  latitude: number;
  longitude: number;
  creationTime?: string;
}
