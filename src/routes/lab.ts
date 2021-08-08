"use strict";
import express, { Request, Response } from "express";

import { R4 } from '@ahryman40k/ts-fhir-types';
import config from '../config';

export const router = express.Router();

router.all('/', async (req: Request, res: Response) => {
  
});

// Create a new lab order in SHR based on bundle 
// (https://i-tech-uw.github.io/emr-lis-ig/Bundle-example-emr-lis-bundle.html)
// router.post('/'), async (req: Request, res: Response) => {
//   logger.info('Received a Lab Order bundle to save');
//   let orderBundle: R4.IBundle = req.body

//   // Validate Bundle
//   if (invalidBundle(orderBundle)) {
//     return res.status(400).json(invalidBundleMessage())
//   }

//   let result: any = await saveBundle(orderBundle)
  
//   return res.status(result.statusCode).json(result.body)
// }

// Get list of active orders targetting :facility
router.get('/orders/target/:facilityId/:_lastUpdated?', (req: Request, res: Response) => {
    return res.status(200).send(req.url);
});

export default router;