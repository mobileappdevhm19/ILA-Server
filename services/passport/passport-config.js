const {Strategy, ExtractJwt} = require('passport-jwt');
const {User} = require('../database/db');

require('dotenv').config();

const secret = process.env.SECRET || '';
if (secret === '') {
    console.error('The SECRET environment variable must be set.');
    process.exit(1);
}

const opts = {
    jwtFromRequest: ExtractJwt.fromAuthHeaderAsBearerToken(),
    secretOrKey: secret
};

module.exports = passport => {
    passport.use(
        new Strategy(opts, (payload, done) => {
            console.log(JSON.stringify(payload));
            User
                .findOne({
                    where: {
                        id: payload.sub
                    }
                })
                .then(user => {
                    if (user) {
                        return done(null, {
                            id: user.id,
                            name: user.name,
                            email: user.email,
                        });
                    }
                    return done(null, false);
                })
                .catch(err => {
                    console.error(err);
                    done(null, false);
                });
        })
    );
};
