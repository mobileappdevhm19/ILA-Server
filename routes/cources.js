const {models} = require('../services/database/db');
const {body, validationResult} = require('express-validator/check');
const crypto = require("crypto");

const router = require('express').Router();

// Returns all owned courses
router.get('/', (req, res) => {
    models.Course
        .findAll({
            where: {ownerId: req.user.id},
            include: [
                {
                    model: models.CourseToken,
                },
                {
                    model: models.User,
                    as: 'Members',
                    attributes: ['id', 'name', 'email'],
                }
            ]
        })
        .then(ownCourses => models.Course
            .findAll({
                include: [
                    {
                        model: models.User,
                        as: 'Members',
                        attributes: [],
                        where: {
                            id: req.user.id
                        }
                    }
                ]
            })
            .then(memberCourses =>
                res.json({
                    ownCourses,
                    memberCourses
                })
            )
        )
        .catch(error => {
            console.error(error);
            res.status(500).json({errors: ['Please ty again. Internal server error.']});
        });
});

// Creates a new course
router.post('/',
    [
        body('title').not().isEmpty().withMessage('title can not be empty.'),
        body('description')
    ],
    (req, res) => {
        const errors = validationResult(req);
        if (!errors.isEmpty()) {
            return res.status(400).json({errors: errors.array()});
        }

        models.Course
            .create({
                ownerId: req.user.id,
                title: req.body.title,
                description: req.body.description
            })
            .then(course => {
                var courseDATA = course.get()
                console.log(JSON.stringify(courseDATA));
                res.json(courseDATA);
            })
            .catch(error => {
                console.error(error);
                res.status(500).json({errors: ['Please ty again. Internal server error.']});
            })
    });

// Update a course
router.put('/',
    [
        body('id').isNumeric().not().isEmpty().withMessage('id can not be empty.'),
        body('title').not().isEmpty().withMessage('title can not be empty.'),
        body('description')
    ],
    (req, res) => {
        const errors = validationResult(req);
        if (!errors.isEmpty()) {
            return res.status(400).json({errors: errors.array()});
        }

        models.Course
            .findByPk(req.body.id)
            .then(course => {
                if (!course) {
                    throw {code: 404, errors: ['Couldn\'t find a course with the id.']};
                }

                if (course.ownerId === req.user.id) {
                    return course
                        .update({
                            title: req.body.title,
                            description: req.body.description
                        })
                        .then(() => res.json(course.get()));
                } else {
                    throw {code: 403, errors: ['You can only change your own course.']};
                }
            })
            .catch(error => {
                console.error(error);
                if (!error || !error.code || error.code === 500)
                    res.status(500).json({errors: ['Please ty again. Internal server error.']});
                else
                    res.status(error.code).json({errors: error.errors});
            });
    });

// Delete a course
router.delete('/',
    [
        body('id').isNumeric().not().isEmpty().withMessage('id can not be empty.')
    ],
    (req, res) => {
        const errors = validationResult(req);
        if (!errors.isEmpty()) {
            return res.status(400).json({errors: errors.array()});
        }

        models.Course
            .findByPk(req.body.id)
            .then(course => {
                if (!course) {
                    throw {code: 404, errors: ['Couldn\'t find a course with the id.']};
                }

                if (course.ownerId === req.user.id) {
                    return course
                        .destroy()
                        .then(() => res.json({}));
                } else {
                    throw {code: 403, errors: ['You can only delete your own course.']};
                }
            })
            .catch(error => {
                console.error(error);
                if (!error || !error.code || error.code === 500)
                    res.status(500).json({errors: ['Please ty again. Internal server error.']});
                else
                    res.status(error.code).json({errors: error.errors});
            });
    });

// Generate Token for course
router.post('/generateToken',
    [
        body('id').isNumeric().not().isEmpty().withMessage('id can not be empty.')
    ],
    (req, res) => {
        const errors = validationResult(req);
        if (!errors.isEmpty()) {
            return res.status(400).json({errors: errors.array()});
        }

        models.Course
            .findByPk(req.body.id)
            .then(course => {
                if (!course) {
                    throw {code: 404, errors: ['Couldn\'t find a course with the id.']};
                }

                if (course.ownerId === req.user.id) {
                    const randomToken = crypto.randomBytes(20).toString('hex').substr(0, 10);
                    return models.CourseToken
                        .create({enabled: true, token: randomToken, courseId: course.id})
                        .then(couresToken => res.json(couresToken));
                } else {
                    throw {code: 403, errors: ['You can only create tokens for your own course.']};
                }
            })
            .catch(error => {
                console.error(error);
                if (!error || !error.code || error.code === 500)
                    res.status(500).json({errors: ['Please ty again. Internal server error.']});
                else
                    res.status(error.code).json({errors: error.errors});
            });
    });

// Join Course
router.post('/join',
    [
        body('courseId').isNumeric().not().isEmpty().withMessage('courseId can not be empty.'),
        body('token').not().isEmpty().withMessage('token can not be empty.')
    ],
    (req, res) => {
        const errors = validationResult(req);
        if (!errors.isEmpty()) {
            return res.status(400).json({errors: errors.array()});
        }

        models.Course
            .findByPk(req.body.courseId, {
                include: [{
                    model: models.CourseToken,
                    where: {
                        token: req.body.token,
                        enabled: true
                    }
                }]
            })
            .then(course => {
                if (!course)
                    throw {code: 404, errors: ['Couldn\'t find a course with the token.']};

                return course
                    .addMember(req.user.id)
                    .then(() => res.json({}));
            })
            .catch(error => {
                console.error(error);
                if (!error || !error.code || error.code === 500)
                    res.status(500).json({errors: ['Please ty again. Internal server error.']});
                else
                    res.status(error.code).json({errors: error.errors});
            });
    });

// Leave course
router.delete('/leave',
    [
        body('id').isNumeric().not().isEmpty().withMessage('id can not be empty.'),],
    (req, res) => {
        const errors = validationResult(req);
        if (!errors.isEmpty()) {
            return res.status(400).json({errors: errors.array()});
        }

        models.Course
            .findByPk(req.body.id, {
                include: [
                    {
                        model: models.User,
                        as: 'Members',
                        attributes: [],
                        where: {
                            id: req.user.id
                        }
                    }
                ]
            })
            .then(course => {
                if (!course)
                    throw {code: 404, errors: ['Couldn\'t find a course.']};

                return course
                    .removeMember(req.user.id)
                    .then(() => res.json({}));
            })
            .catch(error => {
                console.error(error);
                if (!error || !error.code || error.code === 500)
                    res.status(500).json({errors: ['Please ty again. Internal server error.']});
                else
                    res.status(error.code).json({errors: error.errors});
            });
    });

module.exports = router;
