const {User} = require('../services/database/db');
const bcrypt = require('bcryptjs');
const passport = require('passport');
const jwt = require('jsonwebtoken');
const {body, validationResult} = require('express-validator/check');

const router = require('express').Router()

require('dotenv').config();
const secret = process.env.SECRET;
const jwtAudience = process.env.JWT_AUDIENCE || 'https://localhost:3000/';
const jwtIssuer = process.env.JWT_ISSUER || 'https://localhost:3000/';


router.post('/register', [
    body('email')
        .isEmail().withMessage('email must be a valid email.')
        .normalizeEmail(),
    body('password')
        .not().isEmpty().withMessage('passwords can not be empty.')
        .isLength({min: 7, max: 32}).withMessage('the password must be at least 7 characters long.'),
    body('name')
        .not().isEmpty().withMessage('name can not be empty.')
], (req, res) => {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
        return res.status(400).json({errors: errors.array()});
    }

    User
        .count({
            where: {
                email: req.body.email
            }
        })
        .then(c => {
            console.log("Users: " + c);
            if (c > 0)
                throw {code: 400, errors: ['Email Address exists in database.']};
        })
        .then(() => bcrypt.hash(req.body.password, 10))
        .then((hash) => User.create({
            name: req.body.name,
            email: req.body.email,
            // TODO #3: set to false if email validation is implemented
            emailApproved: true,
            password: hash
        }))
        .then(user => {
            // TODO #3: send validation email
        })
        .then(() => res.json({
            messages: [
                'User successful created.',
                // TODO #3: uncomment when email validation is implemented.
                // 'Please check your emails and validate your account.'
            ]
        }))
        .catch(error => {
            console.error(error);
            console.error(JSON.stringify(error));
            if (!error || !error.code || error.code === 500)
                res.status(500).json({errors: ['Please ty again. Internal server error.']});
            else
                res.status(error.code).json({errors: error.errors});
        });
});

router.post('/login',
    [
        body('email')
            .not().isEmpty().withMessage('email can not be empty.'),
        body('password')
            .not().isEmpty().withMessage('passwords can not be empty.')
    ],
    (req, res) => {
        const errors = validationResult(req);
        if (!errors.isEmpty()) {
            return res.status(400).json({errors: errors.array()});
        }

        User
            .findOne({
                where: {
                    email: req.body.email
                }
            })
            .then(user => {
                if (!user) {
                    // TODO #9: log failed login
                    throw {code: 400, errors: ['Username or Password not correct.']};
                }
                // TODO #3: Check if email is approved
                bcrypt.compare(req.body.password, user.password)
                    .then(isMatch => {
                        if (isMatch) {
                            const payload = {
                                id: user.id,
                                name: user.userName
                            };
                            jwt.sign(payload, secret, {
                                    expiresIn: '1h',
                                    issuer: jwtIssuer,
                                    subject: user.id.toString(),
                                    audience: jwtAudience,
                                },
                                (err, token) => {
                                    if (err)
                                        throw err;

                                    // TODO #9: log successful login
                                    res.json({token});
                                })
                        } else {
                            // TODO #9: log failed login
                            throw {code: 400, errors: ['Username or Password not correct.']};
                        }
                    })
            })
            .catch(error => {
                console.error(error);
                console.error(JSON.stringify(error.errors));
                if (!error || !error.code || error.code === 500)
                    res.status(500).json({errors: ['Please ty again. Internal server error.']});
                else
                    res.status(error.code).json({errors: error.errors});
            })
    });

module.exports = router;