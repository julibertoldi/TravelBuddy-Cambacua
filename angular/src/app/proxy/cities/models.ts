
export interface CityDetailDto extends CityDto {
  region?: string;
  latitude: number;
  longitude: number;
  population: number;
}

export interface CityDto {
  id: number;
  name?: string;
  country?: string;
  population: number;
  latitude: number;
  longitude: number;
  imageUrl?: string;
}

export interface CitySearchRequestDto {
  partialName?: string;
  pais?: string;
  region?: string;
  poblacionMinima?: number;
}

export interface CitySearchResultDto {
  cities: CityDto[];
}
