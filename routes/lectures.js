const {models, sequelize} = require('../services/database/db');
const {query, body, validationResult} = require('express-validator/check');

const router = require('express').Router();

// Get Lectures from a course
router.get('/', [
    query('courseId')
        .not().isEmpty().withMessage('courseId can not be empty.')
        .isNumeric()
], (req, res) => {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
        return res.status(400).json({errors: errors.array()});
    }

    sequelize.query('SELECT lectures.* FROM lectures ' +
        'INNER JOIN courses ON courses.id = lectures.courseId ' +
        'INNER JOIN course_members ON course_members.courseId = courses.id AND course_members.userId = ? ' +
        'WHERE lectures.courseId = ?',
        {
            replacements: [
                req.user.id,
                req.query.courseId
            ],
            type: sequelize.QueryTypes.SELECT
        })
        .then(lectures => res.json(lectures))
        .catch(error => {
            console.error(error);
            res.status(500).json({errors: ['Please ty again. Internal server error.']});
        });
});


// Creates a lecture
router.post('/',
    [
        body('courseId')
            .not().isEmpty().withMessage('courseId can not be empty.')
            .isNumeric(),
        body('topic')
            .not().isEmpty().withMessage('topic can not be empty.'),
        body('description'),
        body('visible')
            .not().isEmpty().withMessage('visible can not be empty.')
            .isBoolean()
    ],
    (req, res) => {
        const errors = validationResult(req);
        if (!errors.isEmpty()) {
            return res.status(400).json({errors: errors.array()});
        }

        models.Course
            .findByPk(req.body.courseId)
            .then(course => {
                if (!course) {
                    throw {code: 404, errors: ['Couldn\'t find a course with the id.']};
                }

                if (course.ownerId != req.user.id) {
                    throw {code: 403, errors: ['Couldn\'t add a course where you aren\'t the owner.']};
                }

                return models.Lecture.create({
                    courseId: req.body.courseId,
                    topic: req.body.topic,
                    description: req.body.description,
                    visible: req.body.visible,
                })
                    .then(lecture => res.json(lecture.get().values));
            })
            .catch(error => {
                console.error(error);
                if (!error || !error.code || error.code === 500)
                    res.status(500).json({errors: ['Please ty again. Internal server error.']});
                else
                    res.status(error.code).json({errors: error.errors});
            });
    });

// Delete a lecture
router.delete('/',
    [
        body('id')
            .not().isEmpty().withMessage('id can not be empty.')
            .isNumeric()
    ],
    (req, res) => {
        const errors = validationResult(req);
        if (!errors.isEmpty()) {
            return res.status(400).json({errors: errors.array()});
        }

        models.Lecture
            .findByPk(req.body.id, {include: [models.Course]})
            .then(lecture => {
                if (!lecture) {
                    throw {code: 404, errors: ['Couldn\'t find a lecture with the id.']};
                }

                if (lecture.course.ownerId != req.user.id) {
                    throw {code: 403, errors: ['Couldn\'t delete a lecture where you aren\'t the owner.']};
                }

                return lecture
                    .destroy()
                    .then(() => res.json({}))
            })
            .catch(error => {
                console.error(error);
                if (!error || !error.code || error.code === 500)
                    res.status(500).json({errors: ['Please ty again. Internal server error.']});
                else
                    res.status(error.code).json({errors: error.errors});
            });
    });

// Update a lecture
router.put('/', [
        body('id')
            .not().isEmpty().withMessage('id can not be empty.')
            .isNumeric(),
        body('topic')
            .not().isEmpty().withMessage('topic can not be empty.'),
        body('description')
    ],
    (req, res) => {
        const errors = validationResult(req);
        if (!errors.isEmpty()) {
            return res.status(400).json({errors: errors.array()});
        }

        models.Lecture
            .findByPk(req.body.id, {include: [models.Course]})
            .then(lecture => {
                if (!lecture) {
                    throw {code: 404, errors: ['Couldn\'t find a course with the id.']};
                }

                if (lecture.course.ownerId != req.user.id) {
                    throw {code: 403, errors: ['Couldn\'t delete a lecture where you aren\'t the owner.']};
                }

                return lecture
                    .update({
                        topic: req.body.topic,
                        description: req.body.description,
                    })
                    .then(() => res.json(lecture.get().values));
            })
            .catch(error => {
                console.error(error);
                if (!error || !error.code || error.code === 500)
                    res.status(500).json({errors: ['Please ty again. Internal server error.']});
                else
                    res.status(error.code).json({errors: error.errors});
            });
    });

// Change visibility of a lecture
router.put('/visible', [
        body('id')
            .not().isEmpty().withMessage('id can not be empty.')
            .isNumeric(),
        body('visible')
            .not().isEmpty().withMessage('visible can not be empty.')
            .isBoolean()
    ]
    ,
    (req, res) => {
        const errors = validationResult(req);
        if (!errors.isEmpty()) {
            return res.status(400).json({errors: errors.array()});
        }

        models.Lecture
            .findByPk(req.body.id, {include: [models.Course]})
            .then(lecture => {
                if (!lecture) {
                    throw {code: 404, errors: ['Couldn\'t find a course with the id.']};
                }

                if (lecture.course.ownerId != req.user.id) {
                    throw {code: 403, errors: ['Couldn\'t delete a lecture where you aren\'t the owner.']};
                }

                return lecture
                    .update({visible: req.body.visible})
                    .then(() => res.json(lecture.get().values));
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