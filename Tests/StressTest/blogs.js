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
const token = __ENV.TOKEN ||'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiIwYjRmZGRjZi00ODlkLTQyMTgtOTNlYS1mNTk0ODM1MDA3MTQiLCJzdWIiOiJLZW1vIiwiZW1haWwiOiJrZW1vdmUzMTcyQGhvdGtldi5jb20iLCJqdGkiOiJkZGZlMTJhYy02Y2MwLTRmOTAtYjFmZi0zYzBlNGE0NzBiMDQiLCJyb2xlIjoiUGF0aWVudCIsIm5iZiI6MTc4MjE0NjA2OSwiZXhwIjoxNzgyMTQ2OTY5LCJpYXQiOjE3ODIxNDYwNjksImlzcyI6IkVpcmVuZSIsImF1ZCI6IkV2ZXJ5T25lIn0.a3BcdJ8AlpNj62QaCT0fm_x_A5PB9X8ix9hJmVY-yc0'
export default function () {
    const params = {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
        },
    };

    const res = http.get(`${BASE_URL}/api/Blog`, params);

    check(res, {
        'status is 200': (r) => r.status === 200
    });

    if (res.status !== 200) {
        console.log(`${res.status}: ${res.body}`);
    }

    sleep(1);
}