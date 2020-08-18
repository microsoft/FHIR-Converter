// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------
const _ = require('lodash');

class Interceptor {
    constructor (next) {
        this.__next = next;
    }

    handle (data) {
        if (!!this.__next && this.__next instanceof Interceptor) {
            return this.__next.handle(data);
        }
        return data;
    }
}


class DoNothingInterceptor extends Interceptor {
    handle (data) {
        return super.handle(data);
    }
}


class ExtraDynamicFieldInterceptor extends Interceptor {
    handle (data) {
        if (!_.isPlainObject(data)) {
            return data;
        }
        data = this.__handle(data);
        return super.handle(data);
    }

    __handle (data) {
        if (!('fhirResource' in data && 'entry' in data['fhirResource'])) {
            return data;
        }
        const entries = data['fhirResource']['entry'];
        if (!_.isArray(entries)) {
            return data;
        }
        for (const entry of entries) {
            if ('resource' in entry && 'resourceType' in entry['resource']) {
                if (entry['resource']['resourceType'] === 'DocumentReference') {
                    entry['resource']['date'] = 'removed';
                }
            }
        }
        return data;
    }
}

module.exports = {
    DoNothingInterceptor,
    ExtraDynamicFieldInterceptor,
};