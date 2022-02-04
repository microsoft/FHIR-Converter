"use strict";

import FhirConverter from '../fhirConverter'
import { R4 } from '@ahryman40k/ts-fhir-types'
import got from 'got'

describe('translateBundle', 
  () => { 
    it('should return a HL7v2 Message', async () => { 
      let testBundle: R4.IBundle = await got.get("https://i-tech-uw.github.io/laboratory-workflows-ig/Bundle-example-laboratory-simple-bundle.json").json()

      let fc = new FhirConverter(testBundle, 'OBR.hbs')

      const result = fc.translateBundle();

      expect(result).toContain('|');
      expect(result).toContain(testBundle.entry![1].resource!.id!) 
  }); 
});