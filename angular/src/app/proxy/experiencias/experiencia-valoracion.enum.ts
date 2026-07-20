import { mapEnumToOptions } from '@abp/ng.core';

export enum ExperienciaValoracion {
  Positiva = 1,
  Neutral = 2,
  Negativa = 3,
}

export const experienciaValoracionOptions = mapEnumToOptions(ExperienciaValoracion);
