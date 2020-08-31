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
        const entries = data['entry'];
        if (!_.isArray(entries)) {
            return data;
        }
        for (const entry of entries) {
            if ('resource' in entry && 'resourceType' in entry['resource']) {
                if (entry['resource']['resourceType'] === 'DocumentReference') {
                    const resource = entry['resource'];
                    resource['date'] = 'removed';

                    // The zlib.gzip result will be different on different platforms, see https://stackoverflow.com/questions/26516369/zlib-gzip-produces-different-results-for-same-input-on-different-oses.
                    // Hence the hash result will be different too, which will trigger NodeJS CI error.
                    if ('content' in resource && _.isArray(resource['content'])) {
                        for (const ele of resource['content']) {
                            if ('attachment' in ele) {
                                if ('hash' in ele['attachment']) {
                                    ele['attachment']['hash'] = 'removed-hash';
                                }
                                if ('data' in ele['attachment']) {
                                    ele['attachment']['data'] = 'removed-data';
                                }
                            }
                        }
                    }
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