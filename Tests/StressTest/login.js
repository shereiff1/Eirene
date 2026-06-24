import http from 'k6/http';
import { check, sleep } from 'k6';

export const options = {
    stages: [
        { duration: '30s', target: 50 }, // ramp up to 50 users
        { duration: '1m', target: 50 },  // stay at 50 users
        { duration: '30s', target: 100 }, // ramp up to 100 users
        { duration: '1m', target: 100 },  // stay at 100 users
        { duration: '30s', target: 200 }, // ramp up to 50 users
        { duration: '1m', target: 200 },  // stay at 50 users
        { duration: '30s', target: 400 }, // ramp up to 100 users
        { duration: '1m', target: 400 },  // stay at 100 users
        { duration: '30s', target: 1000 }, // ramp up to 100 users
        { duration: '1m', target: 1000 },  // stay at 100 users
        { duration: '30s', target: 0 },   // scale down
    ],
    thresholds: {
        http_req_duration: ['p(95)<500'], // 95% of requests should be below 500ms
        http_req_failed: ['rate<0.01'],   // less than 1% errors
    },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';

export default function () {
    const payload = JSON.stringify({
        email: 'liyared555@dyleris.com',
        password: '123456SAIUas-@2432578',
    });

    const params = {
        headers: {
            'Content-Type': 'application/json',
        },
    };

    const res = http.post(`${BASE_URL}/api/Auth/login`, payload, params);

    check(res, {
        'status is 200': (r) => r.status === 200,
        'has access token': (r) => {
            try {
                const body = JSON.parse(r.body);
                return body.accessToken !== undefined;
            } catch (e) {
                return false;
            }
        },
    });

    sleep(1);
}